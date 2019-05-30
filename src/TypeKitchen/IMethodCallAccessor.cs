// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace TypeKitchen
{
    public interface IMethodCallAccessor
    {
        string MethodName { get; }
        ParameterInfo[] Parameters { get; }

        object Call(object target);
        object Call(object target, object arg1);
        object Call(object target, object arg1, object arg2);
        object Call(object target, object arg1, object arg2, object arg3);
        object Call(object target, object[] args);
    }
}