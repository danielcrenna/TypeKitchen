
// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace TypeKitchen
{
	public static class TypeExtensions
	{
		private static readonly HashSet<Type> IntegerTypes = new HashSet<Type>
		{
			typeof(sbyte),
			typeof(sbyte?),
			typeof(byte),
			typeof(byte?),
			typeof(ushort),
			typeof(ushort?),
			typeof(short),
			typeof(short?),
			typeof(uint),
			typeof(uint?),
			typeof(int),
			typeof(int?),
			typeof(ulong),
			typeof(ulong?),
			typeof(long),
			typeof(long?)
		};

		private static readonly HashSet<Type> RealNumberTypes = new HashSet<Type>
		{
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(Complex),
			typeof(BigInteger)
		};

		public static bool IsInteger(this Type type)
		{
			return IntegerTypes.Contains(type);
		}

		public static bool IsNumeric(this Type type)
		{
			return RealNumberTypes.Contains(type) || type.IsInteger();
		}

		public static bool IsTruthy(this Type type)
		{
			return type == typeof(bool) || type == typeof(bool?);
		}

		public static bool IsAnonymous(this Type type)
		{
			return type.Namespace == null && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute));
		}
		
		public static string GetPreferredTypeName(this Type type)
		{
			string typeName;

			//
			// Aliases:
			if (type == typeof(string))
				typeName = "string";
			else if (type == typeof(byte))
				typeName = "byte";
			else if (type == typeof(byte?))
				typeName = "byte?";
			else if (type == typeof(bool))
				typeName = "bool";
			else if (type == typeof(bool?))
				typeName = "bool?";
			else if (type == typeof(short))
				typeName = "short";
			else if (type == typeof(short?))
				typeName = "short?";
			else if (type == typeof(ushort))
				typeName = "ushort";
			else if (type == typeof(ushort?))
				typeName = "ushort?";
			else if (type == typeof(int))
				typeName = "int";
			else if (type == typeof(int?))
				typeName = "int?";
			else if (type == typeof(uint))
				typeName = "uint";
			else if (type == typeof(uint?))
				typeName = "uint?";
			else if (type == typeof(long))
				typeName = "long";
			else if (type == typeof(long?))
				typeName = "long?";
			else if (type == typeof(ulong))
				typeName = "ulong";
			else if (type == typeof(ulong?))
				typeName = "ulong?";
			else if (type == typeof(float))
				typeName = "float";
			else if (type == typeof(float?))
				typeName = "float?";
			else if (type == typeof(double))
				typeName = "double";
			else if (type == typeof(double?))
				typeName = "double?";
			else if (type == typeof(decimal))
				typeName = "decimal";
			else if (type == typeof(decimal?))
				typeName = "decimal?";

			//
			// Value Types:
			else if (type.IsValueType())
				typeName = type.Name;
			else if (type.IsNullableValueType())
				typeName = $"{Nullable.GetUnderlyingType(type)?.Name}?";
			else
			{
				//
				// Generics:
				if (type.IsGenericType)
				{
					typeName = Pooling.StringBuilderPool.Scoped(sb =>
					{
						sb.Append(type.GetNonGenericName());
						sb.Append("<");
						for (var i = 0; i < type.GenericTypeArguments.Length; i++)
						{
							if (i != 0)
								sb.Append(",");
							sb.Append(GetPreferredTypeName(type.GenericTypeArguments[i]));
						}
						sb.Append(">");
					});
				}
				else
				{
					typeName = type.Name;
				}
			}

			return typeName;
		}

		public static string GetNonGenericName(this Type type)
		{
			var index = type.Name.IndexOf('`');
			return index == -1 ? type.Name : type.Name.Substring(0, index);
		}

		public static bool IsValueTypeOrNullableValueType(this Type type)
		{
			return type.IsValueType() ||
			       type.IsNullableValueType();
		}

		public static bool IsValueType(this Type type)
		{
			return type.IsPrimitive() ||
			       type.IsEnum ||
			       type == typeof(StringValues) ||
			       type == typeof(DateTime) ||
			       type == typeof(DateTimeOffset) ||
			       type == typeof(TimeSpan) ||
			       type == typeof(Guid);
		}

		public static bool IsNullableValueType(this Type type)
		{
			return type.IsNullablePrimitive() ||
			       type.IsNullableEnum() ||
			       type == typeof(StringValues?) ||
			       type == typeof(DateTime?) ||
			       type == typeof(DateTimeOffset?) ||
			       type == typeof(TimeSpan?) ||
			       type == typeof(Guid?);
		}

		public static bool IsNullableEnum(this Type type)
		{
			return (type = Nullable.GetUnderlyingType(type)) != null && type.IsEnum;
		}

		public static bool IsPrimitiveOrNullablePrimitive(this Type type)
		{
			return type.IsPrimitive() || type.IsNullablePrimitive();
		}

		public static bool IsPrimitive(this Type type)
		{
			return type == typeof(string) ||
			       type == typeof(byte) ||
			       type == typeof(bool) ||
			       type == typeof(short) ||
			       type == typeof(int) ||
			       type == typeof(long) ||
			       type == typeof(float) ||
			       type == typeof(double) ||
			       type == typeof(decimal);
		}

		public static bool IsNullablePrimitive(this Type type)
		{
			return type == typeof(byte?) ||
			       type == typeof(bool?) ||
			       type == typeof(short?) ||
			       type == typeof(int?) ||
			       type == typeof(long?) ||
			       type == typeof(float?) ||
			       type == typeof(double?) ||
			       type == typeof(decimal?);
		}

		public static bool IsAssignableFromGeneric(this Type type, Type c)
		{
			if (!type.IsGenericType)
				return false;

			var interfaceTypes = c.GetInterfaces();

			foreach (var it in interfaceTypes)
			{
				if (it.IsGenericType && it.GetGenericTypeDefinition() == type)
					return true;
			}

			if (c.IsGenericType && c.GetGenericTypeDefinition() == type)
				return true;

			var baseType = c.BaseType;
			return baseType != null && IsAssignableFromGeneric(baseType, type);
		}

		public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true)
			where T : Attribute
		{
			return provider.IsDefined(typeof(T), inherit);
		}

		public static bool TryGetAttribute<T>(this ICustomAttributeProvider provider, bool inherit, out T attribute)
			where T : Attribute
		{
			if (!provider.HasAttribute<T>())
			{
				attribute = default;
				return false;
			}

			foreach (var attr in provider.GetAttributes<T>(inherit))
			{
				attribute = attr;
				return true;
			}

			attribute = default;
			return false;
		}

		public static bool TryGetAttributes<T>(this ICustomAttributeProvider provider, bool inherit,
			out IEnumerable<T> attributes) where T : Attribute
		{
			if (!provider.HasAttribute<T>())
			{
				attributes = Enumerable.Empty<T>();
				return false;
			}

			attributes = provider.GetAttributes<T>(inherit);
			return true;
		}

		public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider provider, bool inherit = true)
			where T : Attribute
		{
			return provider.GetCustomAttributes(typeof(T), inherit).OfType<T>();
		}

		public static IList<T> AsList<T>(this IEnumerable<T> enumerable)
		{
			if (enumerable is IList<T> list)
				return list;
			return enumerable.ToList();
		}
	}
}