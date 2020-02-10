// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TypeKitchen.Composition.Internal;
using TypeKitchen.Creation;
using TypeKitchen.Internal;

namespace TypeKitchen.Composition
{
	public partial class Container
	{
		private readonly Value128 _seed;

		private Container(Value128 seed)
		{
			_seed = seed;
			_context = new UpdateContext(this);
		}

		private readonly List<ISystem> _systems = new List<ISystem>();
		private uint[] _entities = new uint[0];
		private readonly Dictionary<Value128, uint[]> _archetypes = new Dictionary<Value128, uint[]>();

		internal IEnumerable<uint> GetEntities() => _entities;

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

		public Container AddSystem<T>(T system) where T : ISystem
		{
			_systems.Add(system);
			return this;
		}

		private static readonly Dictionary<Type, IEnumerable<Type>> InterfaceMap
			= new Dictionary<Type, IEnumerable<Type>>();


		private static readonly object Sync = new object();

		private IEnumerable<ExecutionPlanLine> BuildExecutionPlan()
		{
			lock(Sync)
			{
				var dependencyMap = new Dictionary<Type, List<ISystem>>();

				foreach (var system in _systems)
				{
					if (!InterfaceMap.TryGetValue(system.GetType(), out var interfaces))
						InterfaceMap.Add(system.GetType(), interfaces = system.GetType().GetTypeInfo().ImplementedInterfaces.AsList());

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
		}

		private readonly UpdateContext _context;

		public UpdateContext Update(UpdateStrategy strategy = UpdateStrategy.Iterative, InactiveHandling inactive = InactiveHandling.Include, ILogger logger = null)
		{
			return Update<object>(logger, strategy, inactive);
		}

		public UpdateContext Update<TState>(TState state = default, UpdateStrategy strategy = UpdateStrategy.Iterative, InactiveHandling inactive = InactiveHandling.Include, ILogger logger = null)
		{
			Tick(state, inactive, logger);

			switch (strategy)
			{
				case UpdateStrategy.Iterative:
					break;
				case UpdateStrategy.Recursive:
					var states = _context.States.ToArray();
					_context.States.Clear();
					foreach (var s in states)
						Tick(s, inactive, logger);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
			}

			_context.Tick();
			return _context;
		}

		private void Tick<TState>(TState state, InactiveHandling inactive, ILogger logger)
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

					var stateType = state?.GetType();
					if (line.System is ISystemWithState && stateType != null &&
					    !line.Parameters[1].ParameterType.IsAssignableFrom(stateType))
						continue;

					logger?.LogDebug(
						$"Executing system '{line.System.GetType().GetPreferredTypeName()}' with state '{state.GetType().GetPreferredTypeName()}'");

					var arguments = Pooling.Arguments.Get(line.Parameters.Length);
					try
					{
						if (!_archetypes.TryGetValue(line.Key, out var array))
							continue; // no entities have this component combination, ignore

						var span = new ReadOnlySpan<uint>(array, 0, array.Length);
						foreach (var entity in span)
						{
							if (_context.InactiveEntities.Contains<Entity>(entity))
							{
								switch (inactive)
								{
									case InactiveHandling.Include:
										break;
									case InactiveHandling.Ignore:
										continue;
									default:
										throw new ArgumentOutOfRangeException(nameof(inactive), inactive, null);
								}
							}

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

								if (state != null && (type == stateType || type.IsAssignableFrom(stateType)))
								{
									arguments[i] = state;
									continue;
								}

								for (var j = 0; j < components.Count; j++)
								{
									var c = components[j];
									var argumentType = type.IsByRef ? type.GetElementType() ?? type : type;
									if (c.GetType() == argumentType)
										arguments[i] = c;
								}
							}

							var update = CallAccessor.Create(line.Update);

							if ((bool) update.Call(line.System, arguments))
							{
								if (!_context.ActiveEntities.Contains<Entity>(entity))
								{
									logger?.LogDebug($"Entity '{entity}' is activated");
									_context.AddActiveEntity(entity);
								}
							}
							else
							{
								logger?.LogDebug($"Entity '{entity}' is de-activated");
								_context.RemoveActiveEntity(entity);
								_context.AddInactiveEntity(entity);
							}

							foreach (var argument in arguments)
							{
								if (argument is UpdateContext || argument is TState && typeof(TState) != typeof(object))
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
				if (c.GetType() != type)
					continue;

				var vm = GetDataForComponentMembers(type);
				var vr = GetDataForComponentReader(type);

				var cm = GetComponentMembers(type);
				var cw = GetComponentWriter(type);

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

		public IEnumerable<ValueType> GetComponents(Entity entity)
		{
			return _componentsByEntity[entity];
		}

		public T GetComponent<T>(Entity entity) where T : struct
		{
			foreach (var c in GetComponents(entity))
			{
				if (c is T t)
					return t;
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
					component = (ValueType) Instancing.CreateInstance(componentType);

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

						// FIXME: whatever.
						// var copy = Cloning.ShallowCopy(value);
						var copy = JsonSerializer.Deserialize(JsonSerializer.Serialize(value), member.Value.Type);
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