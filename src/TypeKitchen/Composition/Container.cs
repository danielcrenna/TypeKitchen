// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TypeKitchen.Internal;

namespace TypeKitchen.Composition
{
	public partial class Container
	{
		public uint CreateEntity<T1>() where T1: struct
		{
			return CreateEntity(typeof(T1));
		}

		public uint CreateEntity<T1, T2>() where T1 : struct where T2 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2));
		}

		public uint CreateEntity(params Type[] componentTypes)
		{
			var stable = componentTypes.NetworkOrder(x => x.Name).ToArray();

			var archetype = componentTypes.Archetype(_seed);

			if (!_entitiesByArchetype.TryGetValue(archetype, out var entities))
				_entitiesByArchetype.Add(archetype, entities = new List<uint>());
			var entity = (uint) EntityIds.IncrementAndGet();
			entities.Add(entity);

			foreach (var component in stable)
			{
				/*
				var members = AccessorMembers.Create(component,
					AccessorMemberTypes.Properties | AccessorMemberTypes.Fields, AccessorMemberScope.Public);
				foreach (var member in members.NetworkOrder(x => x.Name))
				{
					if(member.Type == typeof(float))
					{
						
					}
				}
				*/

				if (!_componentsByEntity.TryGetValue(entity, out var list))
					_componentsByEntity.Add(entity, list = new List<object>());

				var instance = Instancing.CreateInstance(component);
				list.Add(instance);
			}

			IndexArchetypes(stable);

			return entity;
		}
	}

	public partial class Container
	{
		private readonly Value128 _seed;

		private Container(Value128 seed)
		{
			_seed = seed;
		}

		private static readonly AtomicLong EntityIds = new AtomicLong(0);

		
		private readonly List<ISystem> _systems = new List<ISystem>();
		private readonly Dictionary<Value128, List<uint>> _entitiesByArchetype = new Dictionary<Value128, List<uint>>();

		private readonly Dictionary<uint, List<object>> _componentsByEntity = new Dictionary<uint, List<object>>();
		private readonly Dictionary<Value128, Type[]> _componentTypesByArchetype = new Dictionary<Value128, Type[]>();
		
		public static Container Create(Value128 seed = default)
		{
			return new Container(seed);
		}
		
		private void IndexArchetypes(IList<Type> componentTypes)
		{
			for (var i = 1; i < componentTypes.Count + 1; i++)
			{
				IEnumerable<IEnumerable<Type>> combinations = componentTypes.GetCombinations(i);

				foreach (IEnumerable<Type> combination in combinations)
				{
					var types = combination.ToArray();
					_componentTypesByArchetype[types.Archetype(_seed)] = types;
				}
			}
		}

		private IEnumerable<ISystem> _executionPlan;

		public void AddSystem<T>() where T : ISystem, new()
		{
			var system = new T();
			_systems.Add(system);
			_executionPlan = BuildExecutionPlan();
		}

		public void Update()
		{
			foreach (var system in _executionPlan)
			{
				var archetype = system.Archetype(_seed);
				if (!_entitiesByArchetype.TryGetValue(archetype, out var entities))
					continue;

				var method = system.GetType().GetMethod("Update");
				if (method == null)
					continue;

				foreach (var entity in entities)
				{
					var components = _componentsByEntity[entity];

					var parameters = method.GetParameters();
					var arguments = Pooling.Arguments.Get(parameters.Length);
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

						method.Invoke(system, arguments);

						for (var i = 0; i < arguments.Length; i++)
							components[setters[i]] = arguments[i];
					}
					finally
					{
						Pooling.Arguments.Return(arguments);
						Pooling.ListPool<int>.Return(setters);
					}
				}
			}
		}

		private IEnumerable<ISystem> BuildExecutionPlan()
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

			var executionPlan = _systems.OrderBy(x =>
			{
				var index = Array.IndexOf(order, _systems.IndexOf(x));
				return index < 0 ? int.MaxValue : index;
			});

			return executionPlan;
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