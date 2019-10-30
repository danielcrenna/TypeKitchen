// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TypeKitchen.Internal;

namespace TypeKitchen.Composition
{
	public partial class Container
	{
		private readonly Value128 _seed;

		private Container(Value128 seed)
		{
			_seed = seed;
			_context = new UpdateContext();
		}

		private readonly List<ISystem> _systems = new List<ISystem>();
		private uint[] _entities = new uint[0];
		private readonly Dictionary<Value128, uint[]> _archetypes = new Dictionary<Value128, uint[]>();
		
		public static Container Create(Value128 seed = default)
		{
			return new Container(seed);
		}

		private ExecutionPlanLine[] _executionPlan;

		private struct ExecutionPlanLine
		{
			public ISystem System;
			public MethodInfo Update;
			public ParameterInfo[] Parameters;
			public Value128 Key;
		}
		public Container AddSystem<T>() where T : ISystem, new()
		{
			var system = new T();
			_systems.Add(system);
			return this;
		}

		private IEnumerable<ExecutionPlanLine> BuildExecutionPlan()
		{
			var dependencyMap = new Dictionary<Type, List<ISystem>>();

			foreach (var system in _systems)
			{
				var interfaces = system.GetType().GetTypeInfo().ImplementedInterfaces;

				var dependencies = new HashSet<Type>();
				foreach (var dependency in interfaces)
				{
					if (!dependency.IsGenericType)
						continue;

					if (dependency.GetGenericTypeDefinition() != typeof(IDependOn<>))
						continue;

					foreach (var argument in dependency.GetGenericArguments())
						dependencies.Add(argument);
				}

				foreach (var dependency in dependencies)
				{
					if (!dependencyMap.TryGetValue(dependency, out var dependents))
						dependencyMap.Add(dependency, dependents = new List<ISystem>());

					dependents.Add(system);
				}
			}

			var indices = Enumerable.Range(0, _systems.Count).ToList();

			var order = indices.TopologicalSort(i =>
			{
				var s = _systems[i];
				return !dependencyMap.TryGetValue(s.GetType(), out var dependents)
					? Enumerable.Empty<int>()
					: dependents.Select(x => _systems.IndexOf(x));
			}).ToArray();

			foreach (var system in _systems.OrderBy(x =>
			{
				var index = Array.IndexOf(order, _systems.IndexOf(x));
				return index < 0 ? int.MaxValue : index;
			}))
			{
				var archetypes = system.Archetypes(_seed);
				foreach (var (t, v) in archetypes)
				{
					var update = t.GetMethod(nameof(ExecutionPlanLine.Update));
					if (update == null)
						continue;

					var line = new ExecutionPlanLine
					{
						Key = v,
						System = system,
						Update = update,
						Parameters = update.GetParameters()
					};

					yield return line;
				}
			}
		}

		private readonly UpdateContext _context;

		public UpdateContext Update(ILogger logger = null)
		{
			return Update<object>(logger);
		}

		public UpdateContext Update<TState>(TState state = default, ILogger logger = null)
		{
			try
			{
				_context.Reset();
				_executionPlan ??= BuildExecutionPlan().ToArray();

				var executionPlanLines = _executionPlan;
				foreach (var line in executionPlanLines)
				{
					if (line.Key == default)
						continue;

					if (line.Parameters.Length == 0)
						continue;

					var stateType = state.GetType();
					if (line.System is ISystemWithState && stateType != line.Parameters[1].ParameterType)
						continue;

					logger?.LogDebug($"Executing system '{line.System.GetType().GetPreferredTypeName()}' with state '{state.GetType().GetPreferredTypeName()}'");
					
					var arguments = Pooling.Arguments.Get(line.Parameters.Length);
					try
					{
						if (!_archetypes.TryGetValue(line.Key, out var array))
							continue; // no entities have this system, ignore

						var span = new ReadOnlySpan<uint>(array, 0, array.Length);
						foreach (var entity in span)
						{
							logger?.LogDebug($"Entity '{entity}' is associated with this system");

							var components = _componentsByEntity[entity];
							for (var i = 0; i < line.Parameters.Length; i++)
							{
								var type = line.Parameters[i].ParameterType;
								if (type == typeof(UpdateContext))
								{
									arguments[i] = _context;
									continue;
								}
								if (type == stateType || stateType.IsAssignableFrom(type))
								{
									arguments[i] = state;
									continue;
								}

								foreach (var c in components)
								{
									var argumentType = type.IsByRef ? type.GetElementType() ?? type : type;
									if (Proxies[argumentType] != c.GetType())
										continue;

									var argument = c.QuackLike(argumentType);
									arguments[i] = argument;
								}
							}

							if ((bool) line.Update.Invoke(line.System, arguments))
							{
								if (!_context.ActiveEntities.Contains(entity))
								{
									logger?.LogDebug($"Entity '{entity}' is active");
									_context.ActiveEntities.Add(entity);
								}
								else
								{
									logger?.LogDebug($"Entity '{entity}' is inactive");
								}
							}

							foreach (var argument in arguments)
							{
								if (argument is UpdateContext || argument is TState)
									continue;
								var argumentType = argument.GetType();
								if (argumentType.IsByRef)
									argumentType = argumentType.GetElementType();
								SetComponent(entity, argumentType, argument);
							}
						}
					}
					finally
					{
						Pooling.Arguments.Return(arguments);
					}
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine(e);
				throw;
			}

			_context.Tick();
			return _context;
		}

		public void SetComponent<T>(uint entity, T value) where T : struct
		{
			SetComponent(entity, typeof(T), value);
		}

		private void SetComponent(uint entity, Type type, object value)
		{
			for (var i = 0; i < _componentsByEntity[entity].Count; i++)
			{
				var c = _componentsByEntity[entity][i];

				var componentType = Proxies[type];
				if (c.GetType() != componentType)
					continue;

				var vm = GetDataForComponentMembers(type);
				var vr = GetDataForComponentReader(type);

				var cm = GetComponentMembers(componentType);
				var cw = GetComponentWriter(componentType);
				
				foreach (var kvp in vm)
				{
					var member = kvp.Value;

					var componentHasMember = cm.TryGetValue(member.Name, out var m);
					if (!componentHasMember)
						continue;

					if (!m.CanWrite)
						continue;

					var memberTypesMatch = m.Type == member.Type;
					if (!memberTypesMatch)
						continue;

					var valueHasValue = vr.TryGetValue(value, member.Name, out var v);
					if (!valueHasValue)
						continue;

					cw.TrySetValue(c, member.Name, v);
				}
			}
		}
		

		public IEnumerable<IComponentProxy> GetComponents(Entity entity)
		{
			foreach (var component in _componentsByEntity[entity])
				yield return component;
		}

		public void DeleteComponent(Entity entity, IComponentProxy c)
		{
			for (int i = _componentsByEntity[entity].Count - 1; i >= 0; i--)
			{
				var component = _componentsByEntity[entity][i];
				if(component == c)
					_componentsByEntity[entity].RemoveAt(i);
			}
		}

		public T GetComponent<T>(Entity entity) where T : struct
		{
			var key = typeof(T);
			foreach (var c in GetComponents(entity))
			{
				var componentType = key.IsByRef ? key.GetElementType() ?? key : key;
				if (Proxies[componentType] != c.GetType())
					continue;
				return (T) c.QuackLike(componentType);
			}

			return default;
		}

		private uint InitializeEntity(IEnumerable<Type> componentTypes)
		{
			var entity = (uint) _entities.Length + 1;
			Array.Resize(ref _entities, _entities.Length + 1);
			_entities[_entities.Length - 1] = entity;

			var list = componentTypes.ToList();
			for (var i = 0; i < list.Count; i++)
			{
				var k = i + 1;
				foreach (var combination in list.GetCombinations(k))
				{
					var archetype = combination.Archetype(_seed);

					if (!_archetypes.TryGetValue(archetype, out var array))
						_archetypes.Add(archetype, array = new uint[0]);

					Array.Resize(ref array, array.Length + 1);
					array[array.Length - 1] = entity;
					_archetypes[archetype] = array;
				}
			}

			return entity;
		}

		private void CreateComponentProxy(uint entity, Type componentType, object initializer)
		{
			if (!_componentsByEntity.TryGetValue(entity, out var list))
				_componentsByEntity.Add(entity, list = new List<IComponentProxy>());

			var members = GetDataForComponentMembers(componentType);
			var type = GenerateComponentProxy(componentType, members);
			var instance = (IComponentProxy) Activator.CreateInstance(type);

			list.Add(instance);

			if (initializer != null)
				SetComponent(entity, componentType, initializer);
		}

		private static readonly ConcurrentDictionary<Type, Type> Proxies = new ConcurrentDictionary<Type, Type>();
		
		private Type GenerateComponentProxy(Type componentType, Dictionary<string, AccessorMember> members)
		{
			var builder = Snippet.GetBuilder()
				.Add<IComponentProxy>()
				.Add(componentType);

			_configureAction?.Invoke(builder);

			return Proxies.GetOrAdd(componentType, type =>
			{
				builder.Add(type);	

				var code = Pooling.StringBuilderPool.Scoped(sb =>
				{
					Debug.Assert(type.FullName != null, "type.FullName != null");
					sb.AppendLine($"public struct {type.Name}Proxy : IComponentProxy");
					sb.AppendLine("{");
					sb.AppendLine();

					foreach (var kvp in members)
					{
						var member = kvp.Value;

						if (member.Name.EndsWith("k__BackingField"))
							continue;

						builder.Add(member.Type);

						if (member.Type.IsGenericType)
						{
							foreach (var arg in member.Type.GenericTypeArguments)
								builder.Add(arg);
						}

						var alias = member.Type.GetPreferredTypeName();
						sb.AppendLine($"    public {alias} {member.Name} {{ get; set; }}");
					}

					sb.AppendLine("}");
				});

				var proxyType = Snippet.CreateType(code, builder.Build());
				return proxyType; 
			});
		}

		public void Restore(Dictionary<uint, Dictionary<Type, Dictionary<string, object>>> snapshot)
		{
			foreach (var row in snapshot)
			{
				var entity = row.Key;
				var columns = row.Value;

				var components = _componentsByEntity[entity];
				for (var c = components.Count - 1; c >= 0; c--)
				{
					var component = components[c];
					var componentType = component.GetType();

					var members = GetComponentMembers(componentType);
					var accessor = GetComponentWriter(componentType);

					if (!columns.TryGetValue(componentType, out var data))
						continue;

					// create new component instance
					component = Instancing.CreateInstance(componentType) as IComponentProxy;
						
					foreach (var member in members)
					{
						if (!member.Value.CanWrite)
							continue; // skip computed properties

						var memberName = member.Value.Name;
						if (data.TryGetValue(memberName, out var value))
							accessor.TrySetValue(component, memberName, value);
					}
				}
			}
		}

		public Dictionary<uint, Dictionary<Type, Dictionary<string, object>>> Snapshot()
		{
			var projection = Dump();
			var snapshot = projection.ToList();
			return snapshot.ToDictionary(k => k.Key, v => v.Value);
		}

		public IEnumerable<KeyValuePair<uint, Dictionary<Type, Dictionary<string, object>>>> Dump()
		{
			foreach (var entity in _entities)
			{
				var components = GetComponents(entity);
				var rows = new Dictionary<Type, Dictionary<string, object>>();

				foreach (var component in components)
				{
					var componentType = component.GetType();

					var columns = new Dictionary<string, object>();
					rows.Add(componentType, columns);

					var members = GetComponentMembers(componentType);
					var reader = GetComponentReader(componentType);

					foreach (var member in members)
					{
						var memberName = member.Value.Name;
						if (!reader.TryGetValue(component, memberName, out var value))
							continue;

						// whatever.
						// var copy = Cloning.ShallowCopy(value);
						var copy = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), member.Value.Type);
						columns[memberName] = copy;
					}
				}

				yield return new KeyValuePair<uint, Dictionary<Type, Dictionary<string, object>>>(entity, rows);
			}
		}

		private static ITypeReadAccessor GetComponentReader(Type componentType)
		{
			const AccessorMemberTypes types = AccessorMemberTypes.Properties;
			const AccessorMemberScope scope = AccessorMemberScope.Public;
			var accessor = ReadAccessor.Create(componentType, types, scope);
			return accessor;
		}

		private static ITypeWriteAccessor GetComponentWriter(Type componentType)
		{
			const AccessorMemberTypes types = AccessorMemberTypes.Properties;
			const AccessorMemberScope scope = AccessorMemberScope.Public;
			var accessor = WriteAccessor.Create(componentType, types, scope);
			return accessor;
		}

		private static Dictionary<string, AccessorMember> GetComponentMembers(Type componentType)
		{
			const AccessorMemberTypes types = AccessorMemberTypes.Properties;
			const AccessorMemberScope scope = AccessorMemberScope.Public;
			var members = AccessorMembers.Create(componentType, types, scope);
			return members.NetworkOrder(x => x.Name).Reverse().ToDictionary(k => k.Name, v => v);
		}

		private static ITypeReadAccessor GetDataForComponentReader(Type dataType)
		{
			const AccessorMemberTypes types = AccessorMemberTypes.Properties;
			const AccessorMemberScope scope = AccessorMemberScope.All;
			var accessor = ReadAccessor.Create(dataType, types, scope);
			return accessor;
		}

		private static Dictionary<string, AccessorMember> GetDataForComponentMembers(Type dataType)
		{
			const AccessorMemberTypes types = AccessorMemberTypes.Properties;
			const AccessorMemberScope scope = AccessorMemberScope.All;
			var members = AccessorMembers.Create(dataType, types, scope);
			return members.NetworkOrder(x => x.Name).Reverse().ToDictionary(k => k.Name, v => v);
		}
	}
}