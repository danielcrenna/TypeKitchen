// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen
{
    internal static class AccessorMemberScopeExtensions
    {
        public static bool HasFlagFast(this AccessorMemberScope value, AccessorMemberScope flag)
        {
            return (value & flag) != 0;
        }
    }
}