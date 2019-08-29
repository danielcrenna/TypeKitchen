// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
		private readonly Dictionary<uint, List<ValueType>> _componentsByEntity = new Dictionary<uint, List<ValueType>>();

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
				for (var i = 0; i < _entities.Length; i++)
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

				var line = new ExecutionPlanLine
				{
					System = system,
					Update = system.GetType().GetMethod(nameof(ExecutionPlanLine.Update)),
					Start = start.GetValueOrDefault(),
					Length = length
				};

				yield return line;
			}
		}

		public void Update()
		{
			_executionPlan = _executionPlan ?? BuildExecutionPlan();

			foreach (var line in _executionPlan)
			{
				if (line.Length == 0)
					continue;

				var update = line.Update;
				if (update == null)
					continue;

				var parameters = update.GetParameters();
				var arguments = Pooling.Arguments.Get(parameters.Length);
				try
				{
					var entities = new ReadOnlySpan<uint>(_entities, line.Start, line.Length);

					foreach (var entity in entities)
					{
						var components = _componentsByEntity[entity];
						var setters = Pooling.ListPool<int>.Get();
						try
						{
							for (var i = 0; i < parameters.Length; i++)
							{
								var type = parameters[i].ParameterType;
								for (var j = 0; j < components.Count; j++)
								{
									var c = components[j];
									if (c.GetType().MakeByRefType() != type)
										continue;
									arguments[i] = c;
									setters.Add(j);
								}
							}

							update.Invoke(line.System, arguments);

							for (var i = 0; i < arguments.Length; i++)
								components[setters[i]] = (ValueType) arguments[i];
						}
						finally
						{
							Pooling.ListPool<int>.Return(setters);
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
			for (var i = 0; i < _componentsByEntity[entity].Count; i++)
			{
				var component = _componentsByEntity[entity][i];
				if (!(component is T))
					continue;
				_componentsByEntity[entity][i] = value;
				break;
			}
		}

		public IEnumerable<object> GetComponents(Entity entity)
		{
			foreach (var component in _componentsByEntity[entity])
				yield return component;
		}
	}
}