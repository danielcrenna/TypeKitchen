// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace TypeKitchen
{
    public sealed class AccessorMember
    {
        public AccessorMember(string name, Type type, bool canRead, bool canWrite, bool canCall, AccessorMemberScope scope,
            AccessorMemberType memberType, MemberInfo info)
        {
            Name = name;
            Type = type;
            CanRead = canRead;
            CanWrite = canWrite;
            CanCall = canCall;
            Scope = scope;
            MemberType = memberType;
            MemberInfo = info;
        }

        public string Name { get; }
        public Type Type { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
        public bool CanCall { get; }
        public AccessorMemberScope Scope { get; }
        public AccessorMemberType MemberType { get; }
        public MemberInfo MemberInfo { get; }

        internal bool IsInstanceMethod => CanCall && MemberInfo is MethodInfo method && !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") && method.DeclaringType != typeof(object);
    }
}