// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
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

		private readonly Dictionary<uint, List<IProxy>> _componentsByEntity = new Dictionary<uint, List<IProxy>>();

		private int[] _integers = new int[0];
		private float[] _floats = new float[0];

		public uint CreateEntity(params Type[] componentTypes)
		{
			Array.Resize(ref _archetypes, _archetypes.Length + 1);
			Array.Resize(ref _entities, _entities.Length + 1);

			var entity = (uint) _entities.Length + 1;
			var archetype = componentTypes.Archetype(_seed);
			
			_archetypes[_archetypes.Length - 1] = archetype;
			_entities[_entities.Length - 1] = entity;

			foreach (var componentType in componentTypes.NetworkOrder(x => x.Name))
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

						if (member.Type == typeof(int))
						{
							var i = _integers.Length;
							Array.Resize(ref _integers, i + 1);

							var span = new Span<int>(_integers, i, 1);
							arguments[count] = span.GetPinnableReference();
						}

						if (member.Type == typeof(float))
						{
							var i = _floats.Length;
							Array.Resize(ref _floats, i + 1);

							var span = new Span<float>(_floats, i, 1);
							arguments[count] = span.GetPinnableReference();
						}

						count++;
					}

					var instance = (IProxy) Activator.CreateInstance(type, arguments);
					list.Add(instance);
				}
				finally
				{
					Pooling.Arguments.Return(arguments);
				}
			}

			return entity;
		}

		private static readonly ConcurrentDictionary<Type, Type> Proxies = new ConcurrentDictionary<Type, Type>();
		private static Type GenerateComponentProxy(Type componentType, AccessorMembers members)
		{
			return Proxies.GetOrAdd(componentType, type =>
			{
				var code = Pooling.StringBuilderPool.Scoped(sb =>
				{
					sb.AppendLine($"public readonly struct {type.Name}Proxy : IProxy");
					sb.AppendLine("{");
					sb.AppendLine();
					sb.AppendLine($"    public Type RefType => typeof({type.Name}).MakeByRefType();");

					//var count = 0;
					//sb.Append($"    public {type.Name} Ref => new {type.Name} {{");
					//foreach (var member in members)
					//{
					//	if (count != 0)
					//		sb.Append(", ");
					//	sb.Append(member.Name);
					//	sb.Append(" = ");
					//	sb.Append(member.Name);
					//	count++;
					//}
					//sb.AppendLine("};");

					var count = 0;
					sb.AppendLine($"    public unsafe {type.Name} Ref");
					sb.AppendLine("    {");
					sb.AppendLine("        get");
					sb.AppendLine("        {");
					foreach (var member in members)
					{
						var name = member.Name.ToLowerInvariant();
						sb.AppendLine($"            ref var {name} = ref _{name}[0];");
					}
					sb.Append($"            var {type.Name.ToLowerInvariant()} = new {type.Name} {{");
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
