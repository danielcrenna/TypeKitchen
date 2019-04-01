// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
    internal struct AccessorMembersKey : IEquatable<AccessorMembersKey>
    {
        public Type Type { get; }
        public AccessorMemberScope Scope { get; }
        public AccessorMemberTypes Types { get; }

        public AccessorMembersKey(Type type, AccessorMemberScope scope, AccessorMemberTypes types)
        {
            Type = type;
            Scope = scope;
            Types = types;
        }

        public bool Equals(AccessorMembersKey other)
        {
            return Type == other.Type && Scope == other.Scope && Types == other.Types;
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && obj is AccessorMembersKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type != null ? Type.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (int) Scope;
                hashCode = (hashCode * 397) ^ (int) Types;
                return hashCode;
            }
        }

        public static bool operator ==(AccessorMembersKey left, AccessorMembersKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AccessorMembersKey left, AccessorMembersKey right)
        {
            return !left.Equals(right);
        }
    }
}