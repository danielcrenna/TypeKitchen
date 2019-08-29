// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeKitchen.StateMachine;

namespace TypeKitchen.Composition
{
	public class Container
	{
		private readonly Value128 _seed;

		private Container(Value128 seed)
		{
			_seed = seed;
		}

		private static readonly AtomicLong EntityIds = new AtomicLong(0);

		private readonly Dictionary<Value128, Type[]> _componentTypesByArchetype = new Dictionary<Value128, Type[]>();
		private readonly Dictionary<Value128, List<ISystem>> _systemsByArchetype = new Dictionary<Value128, List<ISystem>>();
		private readonly Dictionary<Value128, List<uint>> _entitiesByArchetype = new Dictionary<Value128, List<uint>>();
		private readonly Dictionary<uint, List<object>> _componentsByEntity = new Dictionary<uint, List<object>>();
		
		public static Container Create(Value128 seed = default)
		{
			return new Container(seed);
		}

		public Container CreateEntity(params Type[] componentTypes)
		{
			var stable = componentTypes.NetworkOrder(x => x.Name).ToArray();

			var archetype = GetArchetype(componentTypes);

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

				list.Add((object) Instancing.CreateInstance(component));
			}
			
			IndexArchetypes(stable);
			
			return this;
		}

		private Value128 GetArchetype(IEnumerable<Type> componentTypes)
		{
			Value128 archetype = default;
			foreach (var component in componentTypes.NetworkOrder(x => x.Name))
			{
				var componentId = Hashing.MurmurHash3(component.FullName, _seed);
				archetype = componentId ^ archetype;
			}
			return archetype;
		}

		private void IndexArchetypes(IList<Type> componentTypes)
		{
			for (var i = 1; i < componentTypes.Count + 1; i++)
			{
				IEnumerable<IEnumerable<Type>> combinations = componentTypes.GetCombinations(i);

				foreach (IEnumerable<Type> combination in combinations)
				{
					var array = combination.ToArray();
					var archetype = GetArchetype(array);
					_componentTypesByArchetype[archetype] = array;
				}
			}
		}

		public void AddSystem<T>() where T : ISystem, new()
		{
			var componentTypes = typeof(T).GetTypeInfo().ImplementedInterfaces.Single(x => typeof(ISystem).IsAssignableFrom(x) && x.IsGenericType).GetGenericArguments();
			var archetype = GetArchetype(componentTypes);
			if(!_systemsByArchetype.TryGetValue(archetype, out var systems))
				_systemsByArchetype.Add(archetype, systems = new List<ISystem>());
			var system = new T();
			systems.Add(system);
		}

		public void Update()
		{
			foreach (KeyValuePair<Value128, List<ISystem>> systemList in _systemsByArchetype)
			{
				List<ISystem> systems = systemList.Value;

				if (systems.Count == 0)
					continue;

				Value128 archetype = systemList.Key;
				if (!_entitiesByArchetype.TryGetValue(archetype, out var entities))
					continue;

				foreach (ISystem system in systems)
				{
					var method = system.GetType().GetMethod("Update");
					if (method == null)
						continue;

					foreach (var entity in entities)
					{
						var components = _componentsByEntity[entity];
						var args = components.Cast<object>().ToArray();
						method.Invoke(system, args);
					}
				}
			}
		}

		public IEnumerable<object> GetComponents(Entity entity)
		{
			foreach (var component in _componentsByEntity[entity])
				yield return component;
		}
	}
}