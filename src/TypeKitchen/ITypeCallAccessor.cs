// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
    public interface ITypeCallAccessor
    {
        Type Type { get; }
        object Call(object target, string key, params object[] args);
    }
}