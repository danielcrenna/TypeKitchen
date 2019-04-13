// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace TypeKitchen.Internal
{
    internal static class Methods
    {
        public static readonly MethodInfo StringEquals = typeof(string).GetMethod("op_Equality", new[] { typeof(string), typeof(string) });
        public static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Static | BindingFlags.Public);
    }
}