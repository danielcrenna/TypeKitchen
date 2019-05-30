// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace TypeKitchen.Internal
{
    internal static class KnownMethods
    {
        public static readonly MethodInfo StringEquals =
            typeof(string).GetMethod("op_Equality", new[] {typeof(string), typeof(string)});

        public static readonly MethodInfo GetTypeFromHandle =
            typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Static | BindingFlags.Public);

        public static MethodInfo GetMethodFromHandle =
            typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new[] {typeof(RuntimeMethodHandle)});

        public static MethodInfo GetFieldFromHandle =
            typeof(FieldInfo).GetMethod(nameof(FieldInfo.GetFieldFromHandle), new[] {typeof(RuntimeFieldHandle)});

        public static MethodInfo GetMethodParameters = typeof(MethodInfo).GetMethod(nameof(MethodInfo.GetParameters));

        public static MethodInfo CallWithArgs = typeof(MethodCallAccessor).GetMethod(nameof(MethodCallAccessor.Call),
            new[] {typeof(object), typeof(object[])});
    }
}