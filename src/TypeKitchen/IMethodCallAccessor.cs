// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace TypeKitchen
{
    public interface IMethodCallAccessor
    {
        MethodInfo MethodInfo { get; }
        object Call(object target, params object[] args);
    }
}