// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
		}

		private readonly List<ISystem> _systems = new List<ISystem>();
		private Value128[] _archetypes = new Value128[0];
		private uint[] _entities = new uint[0];
		
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
			public int Start;
			public int Length;
		}

		public void AddSystem<T>() where T : ISystem, new()
		{
			var system = new T();
			_systems.Add(system);
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
				var archetype = system.Archetype(_seed);
				int? start = null;
				var length = 0;
				for (var i = 0; i < _archetypes.Length; i++)
				{
					if (_archetypes[i] == archetype)
					{
						if(!start.HasValue)
							start = i;
						length++;
					}
					else if (start.HasValue)
						break;
				}

				var update = system.GetType().GetMethod(nameof(ExecutionPlanLine.Update));
				if (update == null)
					continue;

				var line = new ExecutionPlanLine
				{
					System = system,
					Update = update,
					Start = start.GetValueOrDefault(),
					Length = length,
					Parameters = update.GetParameters()
				};

				yield return line;
			}
		}

		public void Update()
		{
			Update<object>();
		}

		public void Update<TState>(TState state = default)
		{
			_executionPlan = _executionPlan ?? BuildExecutionPlan();

			foreach (var line in _executionPlan)
			{
				if (line.Length == 0)
					continue;

				var arguments = Pooling.Arguments.Get(line.Parameters.Length);
				try
				{
					var entities = new ReadOnlySpan<uint>(_entities, line.Start, line.Length);

					foreach (var entity in entities)
					{
						var components = _componentsByEntity[entity];
						for (var i = 0; i < line.Parameters.Length; i++)
						{
							var type = line.Parameters[i].ParameterType;
							if (type == typeof(TState))
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

						line.Update.Invoke(line.System, arguments);

						foreach (var argument in arguments)
						{
							var argumentType = argument.GetType();
							if (argumentType == typeof(TState))
								continue;
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

		public void SetComponent<T>(uint entity, T value) where T : struct
		{
			SetComponent(entity, typeof(T), value);
		}

		private void SetComponent(uint entity, Type type, object value)
		{
			for (var i = 0; i < _componentsByEntity[entity].Count; i++)
			{
				var c = _componentsByEntity[entity][i];

				if (c.GetType() != Proxies[type])
					continue;

				const AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties;
				const AccessorMemberScope scope = AccessorMemberScope.Public;

				var cm = AccessorMembers.Create(c, types, scope);
				var vr = ReadAccessor.Create(value, types, scope, out var vm);
				var cw = WriteAccessor.Create(c);
				
				foreach (var member in vm)
				{
					var componentHasMember = cm.TryGetValue(member.Name, out var m);
					var componentCanWrite = m.CanWrite;
					var memberTypesMatch = m.Type == member.Type;
					var valueHasValue = vr.TryGetValue(value, member.Name, out var v);

					if (componentHasMember && componentCanWrite && memberTypesMatch && valueHasValue)
						cw.TrySetValue(c, member.Name, v);
				}
			}
		}

		public IEnumerable<IComponentProxy> GetComponents(Entity entity)
		{
			foreach (var component in _componentsByEntity[entity])
				yield return component;
		}
	}
}