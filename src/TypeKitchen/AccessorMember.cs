// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool HasAttribute<T>() where T : Attribute
        {
            return Attribute.IsDefined(MemberInfo, typeof(T), true);
        }

        public bool TryGetAttribute<T>(out T attribute) where T : Attribute
        {
            if (!Attribute.IsDefined(MemberInfo, typeof(T), true))
            {
                attribute = default;
                return false;
            }
            attribute = Attribute.GetCustomAttribute(MemberInfo, typeof(T), true) as T;
            return attribute != null;
        }

        public bool TryGetAttributes<T>(out Attribute[] attributes) where T : Attribute
        {
            if (!Attribute.IsDefined(MemberInfo, typeof(T), true))
            {
                attributes = default;
                return false;
            }

            var get = Attribute.GetCustomAttributes(MemberInfo, typeof(T), true);
            if (get.Length == 0)
            {
                attributes = default;
                return false;
            }

            attributes = get;
            return true;
        }

        public IEnumerable<T> GetAttributes<T>(bool canInherit = true) where T : Attribute
        {
            return Attribute.GetCustomAttributes(MemberInfo, typeof(T), canInherit).Cast<T>();
        }

        internal bool IsInstanceMethod => CanCall && MemberInfo is MethodInfo method &&
                                          !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") &&
                                           method.DeclaringType != typeof(object);
    }
}