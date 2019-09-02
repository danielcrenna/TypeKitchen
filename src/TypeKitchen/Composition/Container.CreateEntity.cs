// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TypeKitchen.Internal;

namespace TypeKitchen.Composition
{
	partial class Container
	{
		public uint CreateEntity<T1>() where T1 : struct
		{
			return CreateEntity(typeof(T1));
		}

		public uint CreateEntity<T1>(T1 component1) where T1 : struct
		{
			return CreateEntity((object) component1);
		}

		public uint CreateEntity<T1, T2>() where T1 : struct where T2 : struct
		{
			return CreateEntity(typeof(T1), typeof(T2));
		}

		public uint CreateEntity<T1, T2>(T1 component1, T2 component2) where T1 : struct where T2 : struct
		{
			return CreateEntity((object) component1, component2);
		}

		private readonly Dictionary<uint, List<IComponentProxy>> _componentsByEntity = new Dictionary<uint, List<IComponentProxy>>();

		public uint CreateEntity(params Type[] componentTypes)
		{
			var entity = InitializeEntity(componentTypes);
			foreach (var componentType in componentTypes.NetworkOrder(x => x.Name))
				CreateComponentProxy(entity, componentType, null);
			return entity;
		}

		public uint CreateEntity(params object[] components)
		{
			var entity = InitializeEntity(components.Select(x => x.GetType()));
			foreach (var component in components.NetworkOrder(x => x.GetType().Name))
				CreateComponentProxy(entity, component.GetType(), component);
			return entity;
		}

		private uint InitializeEntity(IEnumerable<Type> componentTypes)
		{
			Array.Resize(ref _archetypes, _archetypes.Length + 1);
			Array.Resize(ref _entities, _entities.Length + 1);

			var entity = (uint) _entities.Length;
			var archetype = componentTypes.Archetype(_seed);

			_archetypes[_archetypes.Length - 1] = archetype;
			_entities[_entities.Length - 1] = entity;
			return entity;
		}

		private void CreateComponentProxy(uint entity, Type componentType, object initializer)
		{
			if (!_componentsByEntity.TryGetValue(entity, out var list))
				_componentsByEntity.Add(entity, list = new List<IComponentProxy>());

			var members = AccessorMembers.Create(componentType, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.Public);
			var type = GenerateComponentProxy(componentType, members);
			var instance = (IComponentProxy) Activator.CreateInstance(type);

			list.Add(instance);

			if (initializer != null)
				SetComponent(entity, componentType, initializer);
		}
		
		private static readonly ConcurrentDictionary<Type, Type> Proxies = new ConcurrentDictionary<Type, Type>();
		private static Type GenerateComponentProxy(Type componentType, AccessorMembers members)
		{
			return Proxies.GetOrAdd(componentType, type =>
			{
				var code = Pooling.StringBuilderPool.Scoped(sb =>
				{
					Debug.Assert(type.FullName != null, "type.FullName != null");
					sb.AppendLine($"public struct {type.Name}Proxy : IComponentProxy");
					sb.AppendLine("{");
					sb.AppendLine();
					foreach (var member in members)
					{
						var alias = member.Type.GetPreferredTypeName();
						sb.AppendLine($"    public {alias} {member.Name} {{ get; set; }}");
						sb.AppendLine();
					}
					sb.AppendLine("}");
				});

				var builder = Snippet.GetBuilder()
					.Add<IComponentProxy>()
					.Add(type);

				var proxyType = Snippet.CreateType(code, builder.Build());
				return proxyType;
			});
		}
	}
}
