// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

using IEnumerable = System.Collections.IEnumerable;

namespace TypeKitchen.Serialization
{
	public static class Wire
	{
		public static ReadOnlySpan<byte> ObjectToBuffer(object instance, ITypeResolver typeResolver)
		{
			if (instance == default)
				return default;

			var type = instance.GetType();

			return type.IsValueType || type == typeof(string)
				? ShallowCopyValueTypeData(instance, type, typeResolver)
				: ShallowCopyObjectData(instance, typeResolver);
		}

		public static object BufferToObject(in ReadOnlySpan<byte> buffer, Type type, ITypeResolver typeResolver, IReadObjectSink emitter = null)
		{
			if (buffer == default || buffer.Length == 0 || type == null)
				return default;

			var data = buffer.ToArray(); // FIXME: not necessary if targeting .NET Core

			using var ms = new MemoryStream(data);
			using var br = new BinaryReader(ms);

			return br.ReadObject(type, typeResolver, emitter);
		}
		
		private static ReadOnlySpan<byte> ShallowCopyObjectData(object instance, ITypeResolver typeResolver)
		{
			using var ms = new MemoryStream();
			using var bw = new BinaryWriter(ms);

			WriteObject(bw, instance, typeResolver);
			ms.Position = 0;
			return ms.GetBuffer();
		}
		
		private static ReadOnlySpan<byte> ShallowCopyValueTypeData(object instance, Type type, ITypeResolver typeResolver)
		{
			using var ms = new MemoryStream();
			using var bw = new BinaryWriter(ms);

			WriteValue(bw, type, instance, typeResolver);
			ms.Position = 0;
			return ms.GetBuffer();
		}

		#region Object

		private static void WriteObject(this BinaryWriter bw, object value, ITypeResolver typeResolver)
		{
			if (bw.WriteIsNull(value))
				return;

			var type = value.GetType();
			var members = GetMembers(type);
			
			bw.Write(members.Count);
			foreach (var member in members)
			{
				if (!member.CanWrite)
				{
					bw.WriteIsNull(null);
					continue;
				}

				try
				{
					var reader = GetPropertyReader(type);
					if (!reader.TryGetValue(value, member.Name, out var item))
					{
						bw.WriteIsNull(null);
						continue;
					}

					if (bw.WriteIsNull(item))
						continue;

					var itemType = item.GetType();

					if (itemType.FullName == null)
						throw new NullReferenceException();

					// account for polymorphism
					bw.Write(itemType.FullName);

					if (itemType == member.Type ||						// no conflicts
					    itemType.IsValueType ||							// no complications
					    itemType.IsInterface ||							// needs concrete resolution
					    typeof(IEnumerable).IsAssignableFrom(itemType)	// needs enumeration traversal
					)
					{
						bw.WriteValue(itemType, item, typeResolver);
					}
					else
					{
						bw.WriteObject(item, typeResolver);
					}
				}
				catch (Exception e)
				{
					var json = JsonSerializer.Serialize(value);
					Trace.WriteLine(json);
					Console.WriteLine(e);
					throw;
				}
			}
		}

		private static object ReadObject(this BinaryReader br, Type type, ITypeResolver typeResolver, IReadObjectSink emitter = null)
		{
			if (br.ReadIsNull())
				return null;

			var members = GetMembers(type);

			emitter?.StartedReadingObject(type, members);

			var writer = GetPropertyWriter(type);
			var instance = Instancing.CreateInstance(type);
			
			var count = br.ReadInt32();
			for (var i = 0; i < count; i++)
			{
				if (br.ReadIsNull())
					continue;

				var member = members[i];

				if (!member.CanWrite)
					throw new InvalidOperationException($"{nameof(WriteObject)} wrote an invalid buffer");

				var typeName = br.ReadString();
				var itemType = Type.GetType(typeName) ?? typeResolver?.FindByFullName(typeName);
				if(itemType == null)
					throw new TypeLoadException();

				var item = ReadValue(itemType, br, typeResolver);

				emitter?.ReadMember(type, member.Name, itemType, item);

				writer.TrySetValue(instance, member.Name, item);
			}

			return instance;
		}

		#endregion

		#region Dictionary<T, K>

		private static void WriteTypedDictionary<TKey, TValue>(this BinaryWriter bw, IDictionary<TKey, TValue> dictionary, ITypeResolver typeResolver)
		{
			bw.Write(dictionary.Count);
			foreach (var item in dictionary)
			{
				WriteValue(bw, item.Key.GetType(), item.Key, typeResolver);
				WriteValue(bw, item.Value.GetType(), item.Value, typeResolver);
			}
		}

		private static readonly MethodInfo ReadTypedDictionaryMethod = typeof(Wire).GetMethod(nameof(ReadTypedDictionary), BindingFlags.NonPublic | BindingFlags.Static);
		private static object ReadTypedDictionary<TKey, TValue>(this BinaryReader br, ITypeResolver typeResolver)
		{
			var length = br.ReadInt32();
			var instance = Instancing.CreateInstance<Dictionary<TKey, TValue>>();
			for (var i = 0; i < length; i++)
			{
				var key = (TKey) ReadValue(typeof(TKey), br, typeResolver);
				var value = (TValue) ReadValue(typeof(TValue), br, typeResolver);
				instance.Add(key, value);
			}

			return instance;
		}

		#endregion

		#region Writes

		private static void WriteValue(this BinaryWriter bw, Type type, object value, ITypeResolver typeResolver)
		{
			writeValue:

			if (type == null)
				throw new NullReferenceException();

			switch (value)
			{
				case string v:
					if (!bw.WriteIsNull(value))
						bw.Write(v);
					break;
				case byte v:
					bw.Write(v);
					break;
				case sbyte v:
					bw.Write(v);
					break;
				case bool v:
					bw.WriteBoolean(v);
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
					if (!bw.WriteIsNull(value))
						WriteCharArray(bw, v);
					break;
				case byte[] v:
					if (!bw.WriteIsNull(value))
						WriteByteArray(bw, v);
					break;
				case null:
					bw.WriteIsNull(null);
					break;
				default:

					if (type == typeof(TimeSpan))
					{
						bw.WriteTimeSpan((TimeSpan) value);
						break;
					}

					if (type == typeof(DateTime))
					{
						bw.WriteDateTime(value, typeResolver);
						break;
					}

					if (type == typeof(DateTimeOffset))
					{
						bw.WriteDateTimeOffset((DateTimeOffset) value);
						break;
					}

					if (type == typeof(TimeSpan?))
					{
						if (!bw.WriteIsNull(value))
							bw.WriteTimeSpan((TimeSpan) value);
						break;
					}

					if (type == typeof(DateTime?))
					{
						if (!bw.WriteIsNull(value))
							WriteDateTime(bw, value, typeResolver);
						break;
					}

					if (type == typeof(DateTimeOffset?))
					{
						if (!bw.WriteIsNull(value))
							bw.WriteDateTimeOffset((DateTimeOffset) value);
						break;
					}

					if (typeof(IDictionary<,>).IsAssignableFromGeneric(type))
					{
						if (!bw.WriteIsNull(value))
						{
							var method = typeof(Wire).GetMethod(nameof(WriteTypedDictionary), BindingFlags.NonPublic | BindingFlags.Static);
							if (method == null)
								throw new NullReferenceException();
							var genericMethod = method.MakeGenericMethod(type.GenericTypeArguments);
							genericMethod.Invoke(null, new[] { bw, value, typeResolver });
							break;
						}
					}

					if (typeof(ICollection<>).IsAssignableFromGeneric(type))
					{
						if (!bw.WriteIsNull(value))
						{
							var method = typeof(Wire).GetMethod(nameof(WriteTypedCollection), BindingFlags.NonPublic | BindingFlags.Static);
							if (method == null)
								throw new NullReferenceException();
							var genericMethod = method.MakeGenericMethod(type.GenericTypeArguments);
							genericMethod.Invoke(null, new[] { bw, value, typeResolver });
							break;
						}
					}

					if (typeof(ICollection).IsAssignableFrom(type))
					{
						if (!bw.WriteIsNull(value))
							WriteCollection(bw, (ICollection) value, typeResolver);
						break;
					}

					if (type.IsEnum)
					{
						bw.WriteEnum(value, typeResolver);
						break;
					}

					if (Nullable.GetUnderlyingType(type) != null)
					{
						if (!bw.WriteIsNull(value))
						{
							type = Nullable.GetUnderlyingType(type);
							goto writeValue;
						}
					}

					WriteObject(bw, value, typeResolver);
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
		
		private static void WriteDateTime(this BinaryWriter bw, object value, ITypeResolver typeResolver)
		{
			var date = (DateTime) value;
			WriteValue(bw, typeof(DateTimeKind), date.Kind, typeResolver);
			WriteValue(bw, typeof(DateTime), date.Ticks, typeResolver);
		}

		private static bool WriteIsNull(this BinaryWriter bw, object instance)
		{
			if (instance == null)
				bw.Write((byte) 0);
			else
				bw.Write((byte) 1);

			return instance == null;
		}
		
		private static void WriteEnum(this BinaryWriter bw, object value, ITypeResolver typeResolver)
		{
			var enumType = Enum.GetUnderlyingType(value.GetType());
			var enumValue = Convert.ChangeType(value, enumType);
			WriteValue(bw, enumType, enumValue, typeResolver);
		}
		
		private static void WriteTypedCollection<T>(BinaryWriter bw, ICollection<T> list, ITypeResolver typeResolver)
		{
			bw.Write(list.Count);
			foreach (var item in list)
				WriteValue(bw, item.GetType(), item, typeResolver);
		}

		private static void WriteCollection(this BinaryWriter bw, ICollection list, ITypeResolver typeResolver)
		{
			bw.Write(list.Count);
			foreach (var item in list)
				WriteValue(bw, item.GetType(), item, typeResolver);
		}
		
		#endregion

		#region Reads

		private static object ReadValue(this Type type, BinaryReader br, ITypeResolver typeResolver)
		{
			while (true)
			{
				readValue:

				if (type == null)
					throw new NullReferenceException();

				if (type == typeof(string))
					return br.ReadIsNull() ? default : br.ReadString();
				if (type == typeof(bool))
					return BuiltIns.ReadBoolean(br);
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
					return br.ReadIsNull() ? default : br.ReadByte();
				if (type == typeof(sbyte?))
					return br.ReadIsNull() ? default : br.ReadSByte();
				if (type == typeof(bool?))
					return br.ReadIsNull() ? default : br.ReadBoolean();
				if (type == typeof(short?))
					return br.ReadIsNull() ? default : br.ReadInt16();
				if (type == typeof(ushort?))
					return br.ReadIsNull() ? default : br.ReadUInt16();
				if (type == typeof(int?))
					return br.ReadIsNull() ? default : br.ReadInt32();
				if (type == typeof(uint?))
					return br.ReadIsNull() ? default : br.ReadUInt32();
				if (type == typeof(long?))
					return br.ReadIsNull() ? default : br.ReadInt64();
				if (type == typeof(ulong?))
					return br.ReadIsNull() ? default : br.ReadUInt64();
				if (type == typeof(float?))
					return br.ReadIsNull() ? default : br.ReadSingle();
				if (type == typeof(double?))
					return br.ReadIsNull() ? default : br.ReadDouble();
				if (type == typeof(decimal?))
					return br.ReadIsNull() ? default : br.ReadDecimal();
				if (type == typeof(char?))
					return br.ReadIsNull() ? default : br.ReadChar();

				if (type == typeof(char[]))
					return br.ReadIsNull() ? null : ReadCharArray(br);
				if (type == typeof(byte[]))
					return br.ReadIsNull() ? null : ReadByteArray(br);

				if (type == typeof(TimeSpan))
					return br.ReadTimeSpan();
				if (type == typeof(DateTimeOffset))
					return br.ReadDateTimeOffset();
				if (type == typeof(DateTime))
					return br.ReadDateTime(typeResolver);

				if (type == typeof(TimeSpan?))
					return br.ReadIsNull() ? default : br.ReadTimeSpan();
				if (type == typeof(DateTimeOffset?))
					return br.ReadIsNull() ? null : br.ReadDateTimeOffset();
				if (type == typeof(DateTime?))
					return br.ReadIsNull() ? null : ReadDateTime(br, typeResolver);

				if (typeof(IDictionary<,>).IsAssignableFromGeneric(type))
				{
					if (br.ReadIsNull())
						return null;

					var method = ReadTypedDictionaryMethod;
					if (method == null)
						throw new NullReferenceException();

					var genericMethod = method.MakeGenericMethod(type.GenericTypeArguments);
					return genericMethod.Invoke(null, new object [] { br, typeResolver });
				}

				if (typeof(IList<>).IsAssignableFromGeneric(type))
					return br.ReadIsNull() ? null : br.ReadTypedList(type, typeResolver);
				
				if (typeof(IList).IsAssignableFrom(type))
					return br.ReadIsNull() ? null : br.ReadList(type, typeResolver);

				if (type.IsEnum)
				{
					type = GetEnumType(type);
					goto readValue;
				}

				if (Nullable.GetUnderlyingType(type) != null)
				{
					if (!br.ReadIsNull())
					{
						type = Nullable.GetUnderlyingType(type);
						goto readValue;
					}

					return null;
				}

				return br.ReadObject(type, typeResolver);
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

		private static object ReadDateTime(this BinaryReader br, ITypeResolver typeResolver)
		{
			var kind = (DateTimeKind) ReadValue(GetEnumType(typeof(DateTimeKind)), br, typeResolver);
			var ticks = br.ReadInt64();
			return new DateTime(ticks, kind);
		}

		private static object ReadList(this BinaryReader br, Type type, ITypeResolver typeResolver)
		{
			var length = br.ReadInt32();
			var list = (IList) Instancing.CreateInstance(type);
			var elementType = type.GetElementType();
			for (var i = 0; i < length; i++)
			{
				var item = ReadValue(elementType, br, typeResolver);
				list.Add(item);
			}
			return list;
		}

		private static object ReadTypedList(this BinaryReader br, Type type, ITypeResolver typeResolver)
		{
			var length = br.ReadInt32();

			if (type.IsInterface)
			{
				type = typeof(List<>).MakeGenericType(type.GenericTypeArguments);
			}

			var list = (IList) Instancing.CreateInstance(type);
			var itemType = type.GenericTypeArguments[0];
			for (var i = 0; i < length; i++)
			{
				var item = ReadValue(itemType, br, typeResolver);
				list.Add(item);
			}
			return list;
		}

		private static bool ReadIsNull(this BinaryReader br)
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

		private static ITypeReadAccessor GetPropertyReader(Type type) => ReadAccessor.Create(type, AccessorMemberTypes.Properties);
		private static ITypeWriteAccessor GetPropertyWriter(Type type) => WriteAccessor.Create(type, AccessorMemberTypes.Properties);
		private static AccessorMembers GetMembers(Type type) => AccessorMembers.Create(type, AccessorMemberTypes.Properties);
	}
}