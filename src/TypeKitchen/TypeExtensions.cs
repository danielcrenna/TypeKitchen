// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace TypeKitchen
{
	public static class TypeExtensions
	{
		public static bool IsAnonymous(this Type type)
		{
			return type.Namespace == null && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute));
		}

		public static ConstructorInfo GetWidestConstructor(this Type implementationType)
		{
			var allPublic = implementationType.GetConstructors();
			var constructor = allPublic.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
			return constructor ?? implementationType.GetConstructor(Type.EmptyTypes);
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
				typeName = type.Name;

			return typeName;
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
	}
}