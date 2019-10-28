using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using WyHash;

namespace TypeKitchen
{
	public static class ValueHash
	{
		static ValueHash() =>
			Seed = BitConverter.ToUInt64(
				new[] { (byte) 'd', (byte) 'e', (byte) 'a', (byte) 'd', (byte) 'b', (byte) 'e', (byte) 'e', (byte) 'f' },
				0);

		public static ulong Seed { get; set; }

		public static ulong ComputeHash(object instance, ulong? seed = null)
		{
			seed ??= Seed;

			var accessor = ReadAccessor.Create(instance, out var members);

			using var ms = new MemoryStream();
			using (var bw = new BinaryWriter(ms))
			{
				foreach (var member in members)
				{
					if (member.HasAttribute<CompilerGeneratedAttribute>())
						continue; // backing fields

					if (member.HasAttribute<NotMappedAttribute>() ||
					    member.HasAttribute<IgnoreDataMemberAttribute>())
						continue; // explicitly non-value participating

					WriteValue(accessor[instance, member.Name], member.Type, bw);
				}
			}

			return WyHash64.ComputeHash64(ms.GetBuffer(), seed.Value);
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
					bw.Write(v);
					break;
				case byte[] v:
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

					if (typeof(IEnumerable).IsAssignableFrom(type))
					{
						foreach (var item in (IEnumerable) value)
						{
							WriteValue(item, item.GetType(), bw);
						}

						break;
					}

					bw.Write(ComputeHash(value));
					break;
			}
		}
	}
}