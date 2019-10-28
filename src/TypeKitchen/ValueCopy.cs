// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.IO;

namespace TypeKitchen
{
	public static class ValueCopy
	{
		public static object Copy(object instance)
		{
			if (instance.GetType().IsValueType)
				return instance;

			var copy = Instancing.CreateInstance(instance);

			var read = ReadAccessor.Create(instance, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var readMembers);
			using var source = new MemoryStream();
			using var bw = new BinaryWriter(source);
            foreach (var member in readMembers)
			{
				WriteValue(read[instance, member.Name], member.Type, bw);
			}

            var write = WriteAccessor.Create(instance, out var writeMembers);
            using var target = new MemoryStream();
            using var br = new BinaryReader(target);
            foreach (var member in writeMembers)
            {
	            var value = ReadValue(member.Type, br);
	            write.TrySetValue(copy, member.Name, value);
            }

			return copy;
		}

		private static void WriteValue(object value, Type type, BinaryWriter bw)
		{
			switch (value)
			{
				case string v:
					bw.Write(v);
					break;
				case byte v:
					bw.Write(v);
					break;
				case bool v:
					bw.Write(v);
					break;
				case short v:
					bw.Write(v);
					break;
				case ushort v:
					bw.Write(v);
					break;
				case int v:
					bw.Write(v);
					break;
				case uint v:
					bw.Write(v);
					break;
				case long v:
					bw.Write(v);
					break;
				case ulong v:
					bw.Write(v);
					break;
				case float v:
					bw.Write(v);
					break;
				case double v:
					bw.Write(v);
					break;
				case decimal v:
					bw.Write(v);
					break;
				case char v:
					bw.Write(v);
					break;
				case char[] v:
                    bw.Write(v.Length);
					bw.Write(v);
					break;
				case byte[] v:
					bw.Write(v.Length);
					bw.Write(v);
					break;
				case null:
					bw.Write(0);
					break;
				default:

					if (type.IsEnum)
					{
						var enumType = Enum.GetUnderlyingType(value.GetType());
						var enumValue = Convert.ChangeType(value, enumType);

						WriteValue(enumValue, enumType, bw);
						break;
					}

					if (typeof(IList).IsAssignableFrom(type))
					{
						var length = 0;
						foreach (var item in (IList) value)
						{
							length++;
						}

                        bw.Write(length);
						foreach (var item in (IList) value)
						{
							WriteValue(item, item.GetType(), bw);
						}

						break;
					}

					var read = ReadAccessor.Create(value, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var members);
                    bw.Write(members.Count);
					foreach (var member in members)
					{
						if (read.TryGetValue(value, member.Name, out var item))
						{
                            bw.Write((byte) 1);
							WriteValue(item, item.GetType(), bw);
						}
                        else
                            bw.Write((byte) 0);
					}

					break;
			}
		}

		private static object ReadValue(Type type, BinaryReader br)
		{
			while (true)
			{
				if (type == typeof(string)) return br.ReadString();
				if (type == typeof(byte)) return br.ReadByte();
				if (type == typeof(bool)) return br.ReadBoolean();
				if (type == typeof(short)) return br.ReadInt16();
				if (type == typeof(ushort)) return br.ReadUInt16();
				if (type == typeof(int)) return br.ReadInt32();
				if (type == typeof(uint)) return br.ReadUInt32();
				if (type == typeof(long)) return br.ReadInt64();
				if (type == typeof(ulong)) return br.ReadUInt64();
				if (type == typeof(float)) return br.ReadSingle();
				if (type == typeof(double)) return br.ReadDouble();
				if (type == typeof(decimal)) return br.ReadDecimal();
				if (type == typeof(char)) return br.ReadChar();

				if (type == typeof(char[]))
				{
					var length = br.ReadInt32();
					return br.ReadChars(length);
				}

				if (type == typeof(byte[]))
				{
					var length = br.ReadInt32();
					return br.ReadBytes(length);
				}

				if (type.IsEnum)
				{
					var enumType = Enum.GetUnderlyingType(type);
					type = enumType;
					continue;
				}

				if (typeof(IList).IsAssignableFrom(type))
				{
					var length = br.ReadInt32();
					var enumerable = (IList) Instancing.CreateInstance(type);
					var elementType = type.GetElementType();
					for (var i = 0; i < length; i++)
					{
						var item = ReadValue(elementType, br);
						enumerable.Add(item);
					}

					return enumerable;
				}

				var count = br.ReadInt32();
				var members = AccessorMembers.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
				var write = WriteAccessor.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
				var child = Instancing.CreateInstance(type);
				for (var i = 0; i < count; i++)
				{
					var contains = br.ReadByte();
					if (contains == 0) continue;

					var member = members[i];
					var item = ReadValue(member.Type, br);
					write.TrySetValue(child, member.Name, item);
				}

				return child;
			}
		}
	}
}