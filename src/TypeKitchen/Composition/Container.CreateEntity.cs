using System;
using System.Collections.Generic;
using TypeKitchen.Internal;

namespace TypeKitchen.Composition
{
	partial class Container
	{
		public uint CreateEntity<T1>() where T1 : struct
		{
			return CreateEntity(typeof(T1));
		}

		public uint CreateEntity<T1, T2>() where T1 : struct where T2 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2));
		}

		public uint CreateEntity(params Type[] componentTypes)
		{
			var archetype = componentTypes.Archetype(_seed);

			if (!_entitiesByArchetype.TryGetValue(archetype, out var entities))
				_entitiesByArchetype.Add(archetype, entities = new List<uint>());
			var entity = (uint) entities.Count + 1;
			entities.Add(entity);

			foreach (var component in componentTypes.NetworkOrder(x => x.Name))
			{
				if (!_componentsByEntity.TryGetValue(entity, out var list))
					_componentsByEntity.Add(entity, list = new List<object>());
				var instance = Instancing.CreateInstance(component);
				list.Add(instance);
			}

			return entity;
		}
	}
}
