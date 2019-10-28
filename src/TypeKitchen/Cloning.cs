// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TypeKitchen
{
	public static class Cloning
	{
		public static object ShallowCopy(object instance)
		{
			var type = instance?.GetType();
			if (type == null)
				return null;

			if (type.IsValueType || type == typeof(string))
				return instance;

			if (instance is IEnumerable enumerable)
			{
				switch (instance)
				{
					case IList _:
						return ShallowCopyList(type, enumerable);
					case IDictionary _:
						return ShallowCopyDictionary(type, enumerable);
				}
			}
			
			return ShallowCopyObject(instance);
		}

		private static object ShallowCopyObject(object instance)
		{
			var copy = Instancing.CreateInstance(instance);

			using var buffer = new MemoryStream();

			var read = ReadAccessor.Create(instance, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var readMembers);
			using var bw = new BinaryWriter(buffer);
			foreach (var member in readMembers)
			{
				var value = read[instance, member.Name];
				WriteValue(value, member.Type, bw);
			}

			buffer.Position = 0;

			var write = WriteAccessor.Create(instance, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var writeMembers);
			using var br = new BinaryReader(buffer);
			foreach (var member in readMembers)
			{
				var value = ReadValue(member.Type, br);
				if (!writeMembers.ContainsKey(member.Name))
					continue;
				write.TrySetValue(copy, member.Name, value);
			}

			return copy;
		}

		private static object ShallowCopyList(Type type, IEnumerable enumerable)
		{
			var listType = typeof(IList<>).IsAssignableFromGeneric(type)
				? typeof(List<>).MakeGenericType(type.GenericTypeArguments)
				: type.GetElementType();

			var instance = (IList) Instancing.CreateInstance(listType);
			foreach (var item in enumerable)
			{
				var copy = ShallowCopy(item);
				instance.Add(copy);
			}

			return instance;
		}

		private static object ShallowCopyDictionary(Type type, IEnumerable dictionary)
		{
			var instance = (IDictionary) Instancing.CreateInstance(type);
			var pair = typeof(KeyValuePair<,>).MakeGenericType(type.GenericTypeArguments);

			foreach (var item in dictionary)
			{
				var key = pair.GetProperty("Key")?.GetValue(item);
				if (key == null)
					continue;

				var value = pair.GetProperty("Value")?.GetValue(item);
				var copy = ShallowCopy(value);
				instance.Add(key, copy);
			}

			return instance;
		}

		private static void WriteValue(object value, Type type, BinaryWriter bw)
		{
			switch (value)
			{
				case string v:
					WriteNotNull(bw);
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
					WriteNull(bw);
					break;
				default:

					if (type.IsEnum)
					{
						WriteEnum(value, bw);
						break;
					}

					if (type == typeof(TimeSpan))
					{
						WriteTimeSpan(value, type, bw);
						break;
					}

					if (typeof(ICollection<>).IsAssignableFromGeneric(type))
					{
						var method = typeof(Cloning).GetMethod(nameof(WriteTypedCollection), BindingFlags.NonPublic | BindingFlags.Static);
						if (method == null)
							throw new NullReferenceException();
						var genericMethod = method.MakeGenericMethod(type.GenericTypeArguments);
						genericMethod.Invoke(null, new[] { value, bw });
						break;
					}

					if (typeof(ICollection).IsAssignableFrom(type))
					{
						WriteCollection((ICollection)value, bw);
						break;
					}

					if (typeof(IDictionary<,>).IsAssignableFrom(type))
					{
						var method = typeof(Cloning).GetMethod(nameof(WriteTypedDictionary), BindingFlags.NonPublic | BindingFlags.Static);
						if (method == null)
							throw new NullReferenceException();
						var genericMethod = method.MakeGenericMethod(type.GenericTypeArguments);
						genericMethod.Invoke(null, new[] { value, bw });
						break;
					}

					WriteObject(value, bw);
					break;
			}
		}

		private static bool ReadNotNull(this BinaryReader br)
		{
			return br.ReadByte() != 0;
		}

		private static void WriteNull(this BinaryWriter bw)
		{
			bw.Write((byte) 0);
		}

		private static void WriteNotNull(this BinaryWriter bw)
		{
			bw.Write((byte) 1);
		}

		private static void WriteTimeSpan(object value, Type type, BinaryWriter bw)
		{
			var timespan = (TimeSpan) value;
			WriteValue(timespan.Ticks, type, bw);
		}

		private static void WriteEnum(object value, BinaryWriter bw)
		{
			var enumType = Enum.GetUnderlyingType(value.GetType());
			var enumValue = Convert.ChangeType(value, enumType);
			WriteValue(enumValue, enumType, bw);
		}

		private static void WriteTypedDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary, BinaryWriter bw)
		{
			bw.WriteNotNull();
			bw.Write(dictionary.Count);
			foreach (var item in dictionary)
			{
				WriteValue(item.Key, item.Key.GetType(), bw);
				WriteValue(item.Value, item.Value.GetType(), bw);
			}
		}

		private static void WriteTypedCollection<T>(ICollection<T> list, BinaryWriter bw)
		{
			bw.WriteNotNull();
			bw.Write(list.Count);
			foreach (var item in list)
			{
				WriteValue(item, item.GetType(), bw);
			}
		}

		private static void WriteCollection(ICollection list, BinaryWriter bw)
		{
			bw.WriteNotNull();
			bw.Write(list.Count);
			foreach (var item in list)
			{
				WriteValue(item, item.GetType(), bw);
			}
		}

		private static void WriteObject(object value, BinaryWriter bw)
		{
			bw.WriteNotNull();
			var read = ReadAccessor.Create(value, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var members);
			bw.Write(members.Count);
			foreach (var member in members)
			{
				if (read.TryGetValue(value, member.Name, out var item))
				{
					if (item == null)
					{
						bw.WriteNull();
						continue;
					}

					bw.WriteNotNull();
					WriteValue(item, item.GetType(), bw);
				}
				else
					WriteNull(bw);
			}
		}
		
		private static object ReadValue(Type type, BinaryReader br)
		{
			while (true)
			{
				if (type == typeof(string))
					return ReadNullableString(br);
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
					if (!br.ReadNotNull())
						return null;
					var length = br.ReadInt32();
					return br.ReadChars(length);

				}
				if (type == typeof(byte[]))
				{
					if (!br.ReadNotNull())
						return null;
					var length = br.ReadInt32();
					return br.ReadBytes(length);
				}
				
				if (type.IsEnum)
				{
					type = ReadEnum(type);
					continue;
				}

				if (type == typeof(TimeSpan))
				{
					return ReadTimeSpan(br);
				}

				if (typeof(IList<>).IsAssignableFromGeneric(type))
				{
					return !br.ReadNotNull() ? null : ReadTypedList(type, br);
				}

				if (typeof(IList).IsAssignableFrom(type))
				{
					return !br.ReadNotNull() ? null : ReadList(type, br);
				}

				if (typeof(IDictionary).IsAssignableFrom(type))
				{
					return !br.ReadNotNull() ? null : ReadTypedDictionary(type, br);
				}

				return ReadObject(type, br);
			}
		}

		private static object ReadNullableString(BinaryReader br)
		{
			return br.ReadByte() == 0 ? null : br.ReadString();
		}

		private static object ReadTypedDictionary(Type type, BinaryReader br)
		{
			if (type.IsInterface)
				type = typeof(Dictionary<,>).MakeGenericType(type.GenericTypeArguments);

			var length = br.ReadInt32();
			var instance = (IDictionary) Instancing.CreateInstance(type);
			for (var i = 0; i < length; i++)
			{
				var key = ReadValue(type.GenericTypeArguments[0], br);
				var value = ReadValue(type.GenericTypeArguments[1], br);
				instance.Add(key, value);
			}

			return instance;
		}

		private static object ReadObject(Type type, BinaryReader br)
		{
			if (!br.ReadNotNull())
				return null;

			var count = br.ReadInt32();
			var members = AccessorMembers.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
			var write = WriteAccessor.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
			var instance = Instancing.CreateInstance(type);
			for (var i = 0; i < count; i++)
			{
				var contains = br.ReadByte();
				if (contains == 0)
					continue;

				var member = members[i];
				var item = ReadValue(member.Type, br);
				write.TrySetValue(instance, member.Name, item);
			}

			return instance;
		}

		private static object ReadList(Type type, BinaryReader br)
		{
			var length = br.ReadInt32();
			var list = (IList) Instancing.CreateInstance(type);
			var elementType = type.GetElementType();
			for (var i = 0; i < length; i++)
			{
				var item = ReadValue(elementType, br);
				list.Add(item);
			}
			return list;
		}

		private static object ReadTypedList(Type type, BinaryReader br)
		{
			var length = br.ReadInt32();

			if (type.IsInterface)
				type = typeof(List<>).MakeGenericType(type.GenericTypeArguments);
			
			var list = (IList) Instancing.CreateInstance(type);
			var itemType = type.GenericTypeArguments[0];
			for (var i = 0; i < length; i++)
			{
				var item = ReadValue(itemType, br);
				list.Add(item);
			}
			return list;
		}

		private static object ReadTimeSpan(BinaryReader br)
		{
			var ticks = br.ReadInt64();
			return TimeSpan.FromTicks(ticks);
		}

		private static Type ReadEnum(Type type)
		{
			var enumType = Enum.GetUnderlyingType(type);
			type = enumType;
			return type;
		}
	}
}