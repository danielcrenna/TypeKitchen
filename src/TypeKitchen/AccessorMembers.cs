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
			Types = types;
			Scope = scope;

			NameToMember = new Dictionary<string, AccessorMember>();

			var flags = BindingFlags.Instance | BindingFlags.Static;
			if (scope.HasFlagFast(AccessorMemberScope.Public))
				flags |= BindingFlags.Public;
			if (scope.HasFlagFast(AccessorMemberScope.Private))
				flags |= BindingFlags.NonPublic;

			// First pass: everything
			SetWith(type, types, scope, flags);

			// Second pass: declared only (prefer overrides)
			SetWith(type, types, scope, flags | BindingFlags.DeclaredOnly);

			var fields = FieldInfo ?? Enumerable.Empty<FieldInfo>();
			var properties = PropertyInfo ?? Enumerable.Empty<PropertyInfo>();
			var methods = MethodInfo ?? Enumerable.Empty<MethodInfo>();

			MemberInfo = fields.Cast<MemberInfo>().Concat(properties).Concat(methods).OrderBy(m => m.Name).ToArray();
			Members = NameToMember.Values.OrderBy(m => m.Name).ToList();
		}

		private void SetWith(IReflect type, AccessorMemberTypes types, AccessorMemberScope scope, BindingFlags flags)
		{
			if (types.HasFlagFast(AccessorMemberTypes.Fields))
			{
				FieldInfo = type.GetFields(flags).OrderBy(f => f.Name).ToArray();
				foreach (var field in FieldInfo)
					NameToMember[field.Name] =
						new AccessorMember(field.Name, field.FieldType, true, true, false, scope,
							AccessorMemberType.Field, field);
			}

			if (types.HasFlagFast(AccessorMemberTypes.Properties))
			{
				PropertyInfo = type.GetProperties(flags).OrderBy(p => p.Name).ToArray();
				foreach (var property in PropertyInfo)
					NameToMember[property.Name] =
						new AccessorMember(property.Name, property.PropertyType, property.CanRead, property.CanWrite,
							false, scope, AccessorMemberType.Property, property);
			}

			if (types.HasFlagFast(AccessorMemberTypes.Methods))
			{
				MethodInfo = type.GetMethods(flags).OrderBy(m => m.Name).ToArray();
				foreach (var method in MethodInfo)
					NameToMember[method.Name] =
						new AccessorMember(method.Name, method.ReturnType, false, false, true, scope,
							AccessorMemberType.Method,
							method);
			}
		}

		public Type DeclaringType { get; }
		public AccessorMemberTypes Types { get; }
		public AccessorMemberScope Scope { get; }
		public PropertyInfo[] PropertyInfo { get; set; }
		public FieldInfo[] FieldInfo { get; set; }
		public MethodInfo[] MethodInfo { get; set; }
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