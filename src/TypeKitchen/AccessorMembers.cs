// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
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

		private AccessorMembers(Type type, AccessorMemberTypes types, AccessorMemberScope scope)
		{
			DeclaringType = type;
			NameToMember = new Dictionary<string, AccessorMember>();

			var flags = BindingFlags.Instance;
			if (scope.HasFlagFast(AccessorMemberScope.Public))
				flags |= BindingFlags.Public;
			if (scope.HasFlagFast(AccessorMemberScope.Private))
				flags |= BindingFlags.NonPublic;

			if (types.HasFlagFast(AccessorMemberTypes.Properties))
			{
				PropertyInfo = type.GetProperties(flags).OrderBy(p => p.Name).ToArray();
				foreach (var property in PropertyInfo)
					NameToMember.Add(property.Name,
						new AccessorMember(property.Name, property.PropertyType, property.CanRead, property.CanWrite,
							false, scope, AccessorMemberType.Property, property));
			}

			if (types.HasFlagFast(AccessorMemberTypes.Fields))
			{
				FieldInfo = type.GetFields(flags).OrderBy(f => f.Name).ToArray();
				foreach (var field in FieldInfo)
					NameToMember.Add(field.Name,
						new AccessorMember(field.Name, field.FieldType, true, true, false, scope,
							AccessorMemberType.Field, field));
			}

			if (types.HasFlagFast(AccessorMemberTypes.Methods))
			{
				MethodInfo = type.GetMethods().OrderBy(m => m.Name).ToArray();
				foreach (var method in MethodInfo)
					// this willfully ignores the concept of overloads, last in wins
					NameToMember[method.Name] =
						new AccessorMember(method.Name, method.ReturnType, false, false, true, scope,
							AccessorMemberType.Method,
							method);
			}

			var fields = FieldInfo ?? Enumerable.Empty<FieldInfo>();
			var properties = PropertyInfo ?? Enumerable.Empty<PropertyInfo>();
			var methods = MethodInfo ?? Enumerable.Empty<MethodInfo>();

			MemberInfo = fields.Cast<MemberInfo>().Concat(properties).Concat(methods).OrderBy(m => m.Name).ToArray();
			Members = NameToMember.Values.OrderBy(m => m.Name).ToList();
		}

		public Type DeclaringType { get; }
		public PropertyInfo[] PropertyInfo { get; }
		public FieldInfo[] FieldInfo { get; }
		public MethodInfo[] MethodInfo { get; }
		public MemberInfo[] MemberInfo { get; }
		public List<AccessorMember> Members { get; }

		private Dictionary<string, AccessorMember> NameToMember { get; }

		public AccessorMember this[string name] => NameToMember[name];
		public int Count => NameToMember.Count;
		public IEnumerable<string> Names => NameToMember.Keys;


		public IEnumerator<AccessorMember> GetEnumerator()
		{
			return Members.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool ContainsKey(string key)
		{
			return NameToMember.ContainsKey(key);
		}

		public static AccessorMembers Create(object instance, 
			AccessorMemberTypes types = AccessorMemberTypes.All,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			return instance is Type type
				? Create(type, types, scope)
				: Create(instance.GetType(), types, scope);
		}

		public static AccessorMembers Create(Type type,
			AccessorMemberTypes types = AccessorMemberTypes.All,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			var cacheKey = new AccessorMembersKey(type, types, scope);
			if (!Cache.TryGetValue(cacheKey, out var members))
				Cache.TryAdd(cacheKey, members = new AccessorMembers(type, types, scope));
			return members;
		}

		public bool TryGetValue(string name, out AccessorMember member)
		{
			return NameToMember.TryGetValue(name, out member);
		}
	}
}