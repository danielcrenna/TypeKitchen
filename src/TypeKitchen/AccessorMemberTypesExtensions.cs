// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen
{
    internal static class AccessorMemberTypesExtensions
    {
        public static bool HasFlagFast(this AccessorMemberTypes value, AccessorMemberTypes flag)
        {
            return (value & flag) != 0;
        }
    }
}