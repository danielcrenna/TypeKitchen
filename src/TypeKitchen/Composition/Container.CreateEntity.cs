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

		private readonly Dictionary<uint, List<IProxy>> _componentsByEntity = new Dictionary<uint, List<IProxy>>();

		#region SoA

		private bool[] _bools = new bool[0];
		private byte[] _bytes = new byte[0];
		private sbyte[] _sbytes = new sbyte[0];
		private ushort[] _ushorts = new ushort[0];
		private short[] _shorts = new short[0];
		private uint[] _uints = new uint[0];
		private int[] _ints = new int[0];
		private ulong[] _ulongs = new ulong[0];
		private long[] _longs = new long[0];
		private float[] _floats = new float[0];
		private double[] _doubles = new double[0];
		private decimal[] _decimals = new decimal[0];

		#endregion

		public uint CreateEntity(params Type[] componentTypes)
		{
			var entity = InitializeEntity(componentTypes);
			foreach (var componentType in componentTypes.NetworkOrder(x => x.Name))
				CreateProxy(entity, componentType, null);
			return entity;
		}

		public uint CreateEntity(params object[] components)
		{
			var entity = InitializeEntity(components.Select(x => x.GetType()));
			foreach (var component in components.NetworkOrder(x => x.GetType().Name))
				CreateProxy(entity, component.GetType(), component);
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

		private void CreateProxy(uint entity, Type componentType, object initializer)
		{
			if (!_componentsByEntity.TryGetValue(entity, out var list))
				_componentsByEntity.Add(entity, list = new List<IProxy>());

			var members = AccessorMembers.Create(componentType,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.Public);

			var type = GenerateComponentProxy(componentType, members);

			var arguments = Pooling.Arguments.Get(members.Count);
			try
			{
				var count = 0;
				foreach (var member in members)
				{
					if (!member.Type.IsValueType)
						throw new NotSupportedException("Components do not support mutable structs");

					if (TryMapToMemory(member, _bools, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _sbytes, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _bytes, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _ushorts, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _shorts, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _uints, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _ints, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _ulongs, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _longs, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _floats, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _doubles, arguments, ref count))
						continue;
					if (TryMapToMemory(member, _decimals, arguments, ref count))
						continue;
				}

				var instance = (IProxy) Activator.CreateInstance(type, arguments);

				if(initializer != null)
					SetComponent(entity, componentType, initializer);

				list.Add(instance);
			}
			finally
			{
				Pooling.Arguments.Return(arguments);
			}
		}

		private static bool TryMapToMemory<T>(AccessorMember member, T[] array, IList<object> arguments, ref int count)
		{
			if (member.Type != typeof(T))
				return false;
			var i = array.Length;
			Array.Resize(ref array, i + 1);
			var span = new Span<T>(array, i, 1);
			arguments[count++] = span.GetPinnableReference();
			return true;
		}

		private static readonly ConcurrentDictionary<Type, Type> Proxies = new ConcurrentDictionary<Type, Type>();
		private static Type GenerateComponentProxy(Type componentType, AccessorMembers members)
		{
			return Proxies.GetOrAdd(componentType, type =>
			{
				var code = Pooling.StringBuilderPool.Scoped(sb =>
				{
					Debug.Assert(type.FullName != null, "type.FullName != null");
					var qualifiedName = type.FullName.Replace("+", ".");

					sb.AppendLine($"public readonly struct {type.Name}Proxy : IProxy");
					sb.AppendLine("{");
					sb.AppendLine();
					sb.AppendLine($"    public Type RefType => typeof({qualifiedName}).MakeByRefType();");

					var count = 0;
					sb.AppendLine($"    public unsafe {qualifiedName} Ref");
					sb.AppendLine("    {");
					sb.AppendLine("        get");
					sb.AppendLine("        {");
					foreach (var member in members)
					{
						var name = member.Name.ToLowerInvariant();
						sb.AppendLine($"            ref var {name} = ref _{name}[0];");
					}
					sb.Append($"            var {type.Name.ToLowerInvariant()} = new {qualifiedName} {{");
					foreach (var member in members)
					{
						var name = member.Name.ToLowerInvariant();
						if (count != 0)
							sb.Append(", ");
						sb.Append(member.Name);
						sb.Append(" = ");
						sb.Append(name);
						count++;
					}
					sb.AppendLine("};");
					sb.AppendLine($"            return {type.Name.ToLowerInvariant()};");
					sb.AppendLine("        }");
					sb.AppendLine("    }");

					sb.AppendLine();

					foreach (var member in members)
					{
						if (!member.Type.IsValueType)
							throw new NotSupportedException("Components do not support mutable structs");

						var name = member.Name.ToLowerInvariant();
						var alias = member.Type.GetPreferredTypeName();

						sb.AppendLine($"    private readonly unsafe {alias}* _{name};");
						sb.AppendLine($"    public unsafe {alias} {member.Name}");
						sb.AppendLine($"    {{");
						sb.AppendLine($"        get => _{name}[0];");
						sb.AppendLine($"        set => _{name}[0] = value;");
						sb.AppendLine($"    }}");
						sb.AppendLine();
					}

					sb.Append($"    public unsafe {type.Name}Proxy(");
					count = 0;
					foreach (var member in members)
					{
						var name = member.Name.ToLowerInvariant();
						var alias = member.Type.GetPreferredTypeName();

						if (count != 0)
							sb.Append(", ");
						sb.Append($"ref {alias} {name}");
						count++;
					}

					sb.AppendLine(")");
					sb.AppendLine("    {");
					foreach (var member in members)
					{
						var name = member.Name.ToLowerInvariant();
						var alias = member.Type.GetPreferredTypeName();

						sb.AppendLine($"        _{name} = ({alias}*) Unsafe.AsPointer(ref {name});");
					}

					sb.AppendLine("    }");
					sb.AppendLine("}");
				});

				var builder = Snippet.GetBuilder()
					.Add<IProxy>()
					.Add(type);

				var proxyType = Snippet.CreateType(code, builder.Build());
				return proxyType;
			});
		}
	}
}
