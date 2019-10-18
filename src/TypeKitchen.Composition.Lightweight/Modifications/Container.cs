// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		private IEnumerable<ExecutionPlanLine> _executionPlan;

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
				var dependencies = system.GetType().GetTypeInfo().ImplementedInterfaces
					.Where(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IDependOn<>))
					.SelectMany(y => y.GetGenericArguments());

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

		public UpdateContext Update()
		{
			return Update<object>();
		}

		public UpdateContext Update<TState>(TState state = default)
		{
			try
			{
				_context.Reset();
				_executionPlan = _executionPlan ?? BuildExecutionPlan();

				foreach (var line in _executionPlan)
				{
					if (line.Key == default)
						continue;

					var arguments = Pooling.Arguments.Get(line.Parameters.Length);
					try
					{
						if (!_archetypes.TryGetValue(line.Key, out var array))
							continue; // no entities have this system, ignore

						var span = new ReadOnlySpan<uint>(array, 0, array.Length);
						foreach (var entity in span)
						{
							var components = _componentsByEntity[entity];

							for (var i = 0; i < line.Parameters.Length; i++)
							{
								var type = line.Parameters[i].ParameterType;

								if (type == typeof(UpdateContext))
								{
									arguments[i] = _context;
									continue;
								}

								var stateType = typeof(TState);
								if (type == stateType || stateType.IsAssignableFrom(type))
								{
									arguments[i] = state;
									continue;
								}

								for (var j = 0; j < components.Count; j++)
								{
									var c = components[j];
									var argumentType = type.IsByRef ? type.GetElementType() ?? type : type;
									if (c.GetType() == argumentType)
										arguments[j] = c;
								}
							}

							if ((bool) line.Update.Invoke(line.System, arguments))
							{
								if (!_context.ActiveEntities.Contains(entity))
									_context.ActiveEntities.Add(entity);
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
				Console.WriteLine(e);
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
				if (c.GetType() != type)
					continue;

				var cm = c.GetType().GetMembers().ToDictionary(k => k.Name, v => v);
				var vm = value.GetType().GetMembers().ToDictionary(k => k.Name, v => v);
				
				foreach (var v in vm)
				{
					Type memberType = null;
					var componentCanWrite = false;

					{
						switch (v.Value)
						{
							case PropertyInfo p:
								memberType = p.PropertyType;
								componentCanWrite = p.CanWrite;
								break;
							case FieldInfo f:
								memberType = f.FieldType;
								componentCanWrite = true;
								break;
						}

						if (memberType == null)
							continue;
					}
					
					var componentHasMember = cm.TryGetValue(v.Key, out var m);

					var memberTypesMatch = false;
					switch (m)
					{
						case PropertyInfo p:
							memberTypesMatch = p.PropertyType == memberType;
							break;
						case FieldInfo f:
							memberTypesMatch = f.FieldType == memberType;
							break;
					}

					if (!componentHasMember || !componentCanWrite || !memberTypesMatch)
						continue;

					{
						switch (v.Value)
						{
							case PropertyInfo p:
							{
								var get = p.GetValue(value);
								switch (m)
								{
									case PropertyInfo cp:
										cp.SetValue(c, get);
										break;
									case FieldInfo cf:
										cf.SetValue(c, get);
										break;
								}
								break;
							}
							case FieldInfo f:
							{
								var get = f.GetValue(value);
								switch (m)
								{
									case PropertyInfo cp:
										cp.SetValue(c, get);
										break;
									case FieldInfo cf:
										cf.SetValue(c, get);
										break;
								}
								break;
							}
						}
					}
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
				if (!_componentsByEntity.TryGetValue(entity, out var list))
					continue;
				foreach (var component in list.Where(component => component?.GetType() == c.GetType()))
					return (T) component;
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

		public IEnumerable<object> Dump()
		{
			foreach (var entity in _entities)
			{
				var item = new Dictionary<string, object>();
				var components = GetComponents(entity);
				foreach (var component in components)
				{
					var members = component.GetType().GetMembers();

					foreach(var member in members)
					{
						var memberName = member.Name;

						switch (member)
						{
							case PropertyInfo property:
								item[memberName] = property.GetValue(component);
								break;
							case FieldInfo field:
								item[memberName] = field.GetValue(component);
								break;
						}
					}
				}
				yield return item;
			}
		}
	}
}