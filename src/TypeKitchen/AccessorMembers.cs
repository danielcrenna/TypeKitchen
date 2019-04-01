// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeKitchen
{
    public sealed class AccessorMembers : IEnumerable<AccessorMember>
    {
        private static readonly ConcurrentDictionary<AccessorMembersKey, AccessorMembers> Cache =
            new ConcurrentDictionary<AccessorMembersKey, AccessorMembers>();

        private AccessorMembers(Type type, AccessorMemberScope scope, AccessorMemberTypes memberTypes)
        {
            DeclaringType = type;
            NameToMember = new Dictionary<string, AccessorMember>();

            var flags = BindingFlags.Instance;
            if (scope.HasFlagFast(AccessorMemberScope.Public))
                flags |= BindingFlags.Public;
            if (scope.HasFlagFast(AccessorMemberScope.Private))
                flags |= BindingFlags.NonPublic;

            if (memberTypes.HasFlagFast(AccessorMemberTypes.Properties))
            {
                PropertyInfo = type.GetProperties(flags);
                foreach (var property in PropertyInfo)
                    NameToMember.Add(property.Name,
                        new AccessorMember(property.Name, property.PropertyType, property.CanRead, property.CanWrite,
                            scope, AccessorMemberType.Property, property));
            }

            if (memberTypes.HasFlagFast(AccessorMemberTypes.Fields))
            {
                FieldInfo = type.GetFields(flags);
                foreach (var field in FieldInfo)
                    NameToMember.Add(field.Name,
                        new AccessorMember(field.Name, field.FieldType, true, true, scope, AccessorMemberType.Field,
                            field));
            }

            var fields = FieldInfo ?? Enumerable.Empty<FieldInfo>();
            var properties = PropertyInfo ?? Enumerable.Empty<PropertyInfo>();
            MemberInfo = fields.Cast<MemberInfo>().Concat(properties).ToArray();

            Members = NameToMember.Values.ToArray();
        }

        public Type DeclaringType { get; }
        public PropertyInfo[] PropertyInfo { get; }
        public FieldInfo[] FieldInfo { get; }
        public MemberInfo[] MemberInfo { get; }
        public AccessorMember[] Members { get; }

        private Dictionary<string, AccessorMember> NameToMember { get; }

        public AccessorMember this[string name] => NameToMember[name];

        public IEnumerator<AccessorMember> GetEnumerator()
        {
            return NameToMember.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static AccessorMembers Create(Type type, AccessorMemberScope scope = AccessorMemberScope.All, AccessorMemberTypes memberTypes = AccessorMemberTypes.All)
        {
            var cacheKey = new AccessorMembersKey(type, scope, memberTypes);
            if (!Cache.TryGetValue(cacheKey, out var members))
                 Cache.TryAdd(cacheKey, members = new AccessorMembers(type, scope, memberTypes));
            return members;
        }

        public bool TryGetValue(string name, out AccessorMember member)
        {
            return NameToMember.TryGetValue(name, out member);
        }
    }
}