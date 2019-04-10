// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
    public interface ITypeReadAccessor
    {
        Type Type { get; }
        object this[object target, string key] { get; }
        bool TryGetValue(object target, string key, out object value);
    }
}