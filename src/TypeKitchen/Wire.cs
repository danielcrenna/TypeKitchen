// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TypeKitchen.Internal;

namespace TypeKitchen
{
	public static class Wire
	{
		public static byte[] Simple(object instance)
		{
			if (instance == null)
				return null;

			var type = instance.GetType();
			if (type.IsValueType || type == typeof(string))
				return ShallowCopyValueTypeData(instance, type);
			else
				return ShallowCopyObjectData(instance);
		}

		private static byte[] ShallowCopyObjectData(object instance)
		{
			var read = ReadAccessor.Create(instance, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var readMembers);

			using (var buffer = new MemoryStream())
			{
				using (var bw = new BinaryWriter(buffer))
				{
					foreach (var member in readMembers.NetworkOrder(x => x.Name))
					{
						var value = read[instance, member.Name];
						WriteValue(value, member.Type, bw);
					}

					buffer.Position = 0;
					return buffer.ToArray();
				}
			}
		}

		private static byte[] ShallowCopyValueTypeData(object instance, Type type)
		{
			using (var buffer = new MemoryStream())
			{
				using (var bw = new BinaryWriter(buffer))
				{
					WriteValue(instance, type, bw);
					buffer.Position = 0;
					return buffer.ToArray();
				}
			}
		}
		
		#region Writes

		private static void WriteValue(object value, Type type, BinaryWriter bw)
		{
			writeValue:

			switch (value)
			{
				case string v:
					if (!bw.WriteNull(value))
						bw.Write(v);
					break;
				case byte v:
					bw.Write(v);
					break;
				case sbyte v:
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
					if (!bw.WriteNull(value))
						WriteCharArray(bw, v);
					break;
				case byte[] v:
					if (!bw.WriteNull(value))
						WriteByteArray(bw, v);
					break;
				case null:
					bw.WriteNull(null);
					break;
				default:

					if (type == typeof(TimeSpan))
					{
						bw.WriteTimeSpan(value);
						break;
					}

					if (type == typeof(DateTime))
					{
						bw.WriteDateTime(value);
						break;
					}

					if (type == typeof(DateTimeOffset))
					{
						bw.WriteDateTimeOffset(value);
						break;
					}

					if (type == typeof(TimeSpan?))
					{
						if (!bw.WriteNull(value))
							WriteTimeSpan(bw, value);
						break;
					}

					if (type == typeof(DateTime?))
					{
						if (!bw.WriteNull(value))
							WriteDateTime(bw, value);
						break;
					}

					if (type == typeof(DateTimeOffset?))
					{
						if (!bw.WriteNull(value))
							bw.WriteDateTimeOffset(value);
						break;
					}

					if (typeof(IDictionary<,>).IsAssignableFromGeneric(type))
					{
						if (!bw.WriteNull(value))
						{
							var method = typeof(Wire).GetMethod(nameof(WriteTypedDictionary), BindingFlags.NonPublic | BindingFlags.Static);
							if (method == null)
								throw new NullReferenceException();
							var genericMethod = method.MakeGenericMethod(type.GenericTypeArguments);
							genericMethod.Invoke(null, new[] { bw, value });
							break;
						}
					}

					if (typeof(ICollection<>).IsAssignableFromGeneric(type))
					{
						if (!bw.WriteNull(value))
						{
							var method = typeof(Wire).GetMethod(nameof(WriteTypedCollection), BindingFlags.NonPublic | BindingFlags.Static);
							if (method == null)
								throw new NullReferenceException();
							var genericMethod = method.MakeGenericMethod(type.GenericTypeArguments);
							genericMethod.Invoke(null, new[] { bw, value });
							break;
						}
					}

					if (typeof(ICollection).IsAssignableFrom(type))
					{
						if (!bw.WriteNull(value))
							WriteCollection(bw, (ICollection) value);
						break;
					}

					if (type.IsEnum)
					{
						bw.WriteEnum(value);
						break;
					}

					if (Nullable.GetUnderlyingType(type) != null)
					{
						if (!bw.WriteNull(value))
						{
							type = Nullable.GetUnderlyingType(type);
							goto writeValue;
						}
					}

					if (!bw.WriteNull(value))
						WriteObject(bw, value);

					break;
			}
		}

		private static void WriteByteArray(this BinaryWriter bw, byte[] value)
		{
			bw.Write(value.Length);
			bw.Write(value);
		}

		private static void WriteCharArray(this BinaryWriter bw, char[] value)
		{
			bw.Write(value.Length);
			bw.Write(value);
		}

		private static void WriteDateTimeOffset(this BinaryWriter bw, object value)
		{
			var date = (DateTimeOffset) value;
			WriteTimeSpan(bw, date.Offset);
			WriteValue(date.Ticks, typeof(DateTimeOffset), bw);
		}

		private static void WriteDateTime(this BinaryWriter bw, object value)
		{
			var date = (DateTime) value;
			WriteValue(date.Kind, typeof(DateTimeKind), bw);
			WriteValue(date.Ticks, typeof(DateTime), bw);
		}

		private static bool WriteNull(this BinaryWriter bw, object instance)
		{
			if (instance == null)
				bw.Write((byte) 0);
			else
				bw.Write((byte) 1);

			return instance == null;
		}

		private static void WriteContains(this BinaryWriter bw)
		{
			bw.Write((byte) 1);
		}
        
		private static void WriteNotContains(BinaryWriter bw)
		{
			bw.Write((byte) 0);
		}

		private static void WriteTimeSpan(this BinaryWriter bw, object value)
		{
			var timespan = (TimeSpan) value;
			WriteValue(timespan.Ticks, typeof(TimeSpan), bw);
		}

		private static void WriteEnum(this BinaryWriter bw, object value)
		{
			var enumType = Enum.GetUnderlyingType(value.GetType());
			var enumValue = Convert.ChangeType(value, enumType);
			WriteValue(enumValue, enumType, bw);
		}

		private static void WriteTypedDictionary<TKey, TValue>(this BinaryWriter bw, IDictionary<TKey, TValue> dictionary)
		{
			bw.Write(dictionary.Count);
			foreach (var item in dictionary)
			{
				WriteValue(item.Key, item.Key.GetType(), bw);
				WriteValue(item.Value, item.Value.GetType(), bw);
			}
		}

		private static void WriteTypedCollection<T>(BinaryWriter bw, ICollection<T> list)
		{
			bw.Write(list.Count);
			foreach (var item in list)
				WriteValue(item, item.GetType(), bw);
		}

		private static void WriteCollection(this BinaryWriter bw, ICollection list)
		{
			bw.Write(list.Count);
			foreach (var item in list)
				WriteValue(item, item.GetType(), bw);
		}

		private static void WriteObject(this BinaryWriter bw, object value)
		{
			var read = ReadAccessor.Create(value, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var members);
			bw.Write(members.Count);
			foreach (var member in members)
			{
				if (!read.TryGetValue(value, member.Name, out var item) || item == null)
					WriteNotContains(bw);
				else
				{
					bw.WriteContains();
					WriteValue(item, item.GetType(), bw);
				}
			}
		}
        
		#endregion

		#region Reads

		internal static object ReadValue(this Type type, BinaryReader br)
		{
			while (true)
			{
				readValue:

				if (type == typeof(string))
					return br.ReadNull() ? default : br.ReadString();
				if (type == typeof(bool))
					return br.ReadBoolean();
				if (type == typeof(int))
					return br.ReadInt32();
				if (type == typeof(long))
					return br.ReadInt64();
				if (type == typeof(float))
					return br.ReadSingle();
				if (type == typeof(double))
					return br.ReadDouble();
				if (type == typeof(decimal))
					return br.ReadDecimal();
				if (type == typeof(short))
					return br.ReadInt16();
				if (type == typeof(uint))
					return br.ReadUInt32();
				if (type == typeof(ulong))
					return br.ReadUInt64();
				if (type == typeof(ushort))
					return br.ReadUInt16();
				if (type == typeof(byte))
					return br.ReadByte();
				if (type == typeof(sbyte))
					return br.ReadSByte();
				if (type == typeof(char))
					return br.ReadChar();

				if (type == typeof(byte?))
					return br.ReadNull() ? default : br.ReadByte();
				if (type == typeof(sbyte?))
					return br.ReadNull() ? default : br.ReadSByte();
				if (type == typeof(bool?))
					return br.ReadNull() ? default : br.ReadBoolean();
				if (type == typeof(short?))
					return br.ReadNull() ? default : br.ReadInt16();
				if (type == typeof(ushort?))
					return br.ReadNull() ? default : br.ReadUInt16();
				if (type == typeof(int?))
					return br.ReadNull() ? default : br.ReadInt32();
				if (type == typeof(uint?))
					return br.ReadNull() ? default : br.ReadUInt32();
				if (type == typeof(long?))
					return br.ReadNull() ? default : br.ReadInt64();
				if (type == typeof(ulong?))
					return br.ReadNull() ? default : br.ReadUInt64();
				if (type == typeof(float?))
					return br.ReadNull() ? default : br.ReadSingle();
				if (type == typeof(double?))
					return br.ReadNull() ? default : br.ReadDouble();
				if (type == typeof(decimal?))
					return br.ReadNull() ? default : br.ReadDecimal();
				if (type == typeof(char?))
					return br.ReadNull() ? default : br.ReadChar();

				if (type == typeof(char[]))
					return br.ReadNull() ? null : ReadCharArray(br);
				if (type == typeof(byte[]))
					return br.ReadNull() ? null : ReadByteArray(br);

				if (type == typeof(TimeSpan))
					return br.ReadTimeSpan();
				if (type == typeof(DateTimeOffset))
					return br.ReadDateTimeOffset();
				if (type == typeof(DateTime))
					return br.ReadDateTime();

				if (type == typeof(TimeSpan?))
					return br.ReadNull() ? null : ReadTimeSpan(br);
				if (type == typeof(DateTimeOffset?))
					return br.ReadNull() ? null : ReadDateTimeOffset(br);
				if (type == typeof(DateTime?))
					return br.ReadNull() ? null : ReadDateTime(br);

				if (typeof(IDictionary).IsAssignableFrom(type))
					return br.ReadNull() ? null : ReadTypedDictionary(type, br);
				if (typeof(IList<>).IsAssignableFromGeneric(type))
					return br.ReadNull() ? null : ReadTypedList(type, br);
				if (typeof(IList).IsAssignableFrom(type))
					return br.ReadNull() ? null : ReadList(type, br);

				if (type.IsEnum)
				{
					type = GetEnumType(type);
					goto readValue;
				}

				if (Nullable.GetUnderlyingType(type) != null)
				{
					if (!br.ReadNull())
					{
						type = Nullable.GetUnderlyingType(type);
						goto readValue;
					}

					return null;
				}

                if(!br.ReadNull())
					return ReadObject(type, br);
			}
		}

		private static object ReadByteArray(BinaryReader br)
		{
			var length = br.ReadInt32();
			return br.ReadBytes(length);
		}

		private static object ReadCharArray(BinaryReader br)
		{
			var length = br.ReadInt32();
			return br.ReadChars(length);
		}

		private static object ReadDateTime(this BinaryReader br)
		{
			var kind = (DateTimeKind) ReadValue(GetEnumType(typeof(DateTimeKind)), br);
			var ticks = br.ReadInt64();
			return new DateTime(ticks, kind);
		}

		private static object ReadDateTimeOffset(this BinaryReader br)
		{
			var offset = (TimeSpan) ReadTimeSpan(br);
			var ticks = br.ReadInt64();
			return new DateTimeOffset(new DateTime(ticks), offset);
		}

		private static object ReadTypedDictionary(this Type type, BinaryReader br)
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

		private static object ReadObject(this Type type, BinaryReader br)
		{
			var count = br.ReadInt32();
			var members = AccessorMembers.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
			var write = WriteAccessor.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
			var instance = Instancing.CreateInstance(type);
			for (var i = 0; i < count; i++)
			{
				if (!br.ReadContains())
					continue;

				var member = members[i];
				var item = ReadValue(member.Type, br);
				write.TrySetValue(instance, member.Name, item);
			}

			return instance;
		}

		private static bool ReadContains(this BinaryReader br)
		{
			return br.ReadByte() == 1;
		}

		private static object ReadList(this Type type, BinaryReader br)
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

		private static object ReadTypedList(this Type type, BinaryReader br)
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

		private static object ReadTimeSpan(this BinaryReader br)
		{
			return TimeSpan.FromTicks(br.ReadInt64());
		}

		private static bool ReadNull(this BinaryReader br)
		{
			return br.ReadByte() == 0;
		}

		#endregion

		private static Type GetEnumType(Type type)
		{
			var enumType = Enum.GetUnderlyingType(type);
			type = enumType;
			return type;
		}
	}
}