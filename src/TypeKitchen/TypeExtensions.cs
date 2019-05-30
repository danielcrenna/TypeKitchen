// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
                typeName = $"{type.Name}?";
            else
                typeName = type.Name;

            return typeName;
        }

        public static bool IsValueTypeOrNullableValueType(this Type type)
        {
            return type.IsPrimitiveOrNullablePrimitive() ||
                   type == typeof(StringValues) ||
                   type == typeof(StringValues?) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTime?) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(DateTimeOffset?) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(TimeSpan?) ||
                   type == typeof(Guid) ||
                   type == typeof(Guid?);
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsPrimitive() ||
                   type == typeof(StringValues) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }

        public static bool IsNullableValueType(this Type type)
        {
            return type.IsNullablePrimitive() ||
                   type == typeof(StringValues?) ||
                   type == typeof(DateTime?) ||
                   type == typeof(DateTimeOffset?) ||
                   type == typeof(TimeSpan?) ||
                   type == typeof(Guid?);
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
    }
}