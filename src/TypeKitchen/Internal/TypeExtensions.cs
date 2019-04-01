// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace TypeKitchen.Internal
{
    internal static class TypeExtensions
    {
        public static bool IsAnonymous(this Type type)
        {
            return type.Namespace == null && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute));
        }
    }
}