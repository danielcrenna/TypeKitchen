// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
			Array.Resize(ref _archetypes, _archetypes.Length + 1);
			Array.Resize(ref _entities, _entities.Length + 1);

			var entity = (uint) _entities.Length + 1;
			var archetype = componentTypes.Archetype(_seed);
			
			_archetypes[_archetypes.Length - 1] = archetype;
			_entities[_entities.Length - 1] = entity;

			foreach (var component in componentTypes.NetworkOrder(x => x.Name))
			{
				if (!_componentsByEntity.TryGetValue(entity, out var list))
					_componentsByEntity.Add(entity, list = new List<ValueType>());
				var instance = (ValueType) Instancing.CreateInstance(component);
				list.Add(instance);
			}

			return entity;
		}
	}
}
