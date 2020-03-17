// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
			DisplayName = GetDisplayName();
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

			MemberInfo = fields.Cast<MemberInfo>().Concat(properties).Concat(methods).OrderBy(TryOrderMemberInfo)
				.ToArray();
			Members = NameToMember.Values.OrderBy(TryOrderMember).ToList();
		}

		public string DisplayName { get; }

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
		public AccessorMember this[int index] => Members[index];

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

		private static object TryOrderMemberInfo(MemberInfo m)
		{
			if (m.TryGetAttribute(true, out DisplayAttribute display) && display.GetOrder().HasValue)
			{
				return display.GetOrder().GetValueOrDefault();
			}

			return m.Name;
		}

		private static object TryOrderMember(AccessorMember m)
		{
			if (m.TryGetAttribute(out DisplayAttribute display) && display.GetOrder().HasValue)
			{
				return display.GetOrder().GetValueOrDefault();
			}

			return m.Name;
		}

		private void SetWith(Type type, AccessorMemberTypes types, AccessorMemberScope scope, BindingFlags flags)
		{
			if (types.HasFlagFast(AccessorMemberTypes.Fields))
			{
				var fields = type.GetFields(flags).OrderBy(f => f.Name);
				FieldInfo = FieldInfo == null ? fields.ToArray() : FieldInfo.Concat(fields).Distinct().ToArray();
				foreach (var field in FieldInfo)
					NameToMember[field.Name] =
						new AccessorMember(type, field.Name, field.FieldType, true, true, false, scope,
							AccessorMemberType.Field, field);
			}

			if (types.HasFlagFast(AccessorMemberTypes.Properties))
			{
				var properties = type.GetProperties(flags).OrderBy(p => p.Name);
				PropertyInfo = PropertyInfo == null
					? properties.ToArray()
					: PropertyInfo.Concat(properties).Distinct().ToArray();
				foreach (var property in PropertyInfo)
					NameToMember[property.Name] =
						new AccessorMember(type, property.Name, property.PropertyType, CanAccessorRead(property, scope),
							CanAccessorWrite(property, scope),
							false, scope, AccessorMemberType.Property, property);
			}

			if (types.HasFlagFast(AccessorMemberTypes.Methods))
			{
				var methods = type.GetMethods(flags).OrderBy(m => m.Name);
				MethodInfo = MethodInfo == null ? methods.ToArray() : MethodInfo.Concat(methods).Distinct().ToArray();
				foreach (var method in MethodInfo)
					NameToMember[method.Name] =
						new AccessorMember(type, method.Name, method.ReturnType, false, false, true, scope,
							AccessorMemberType.Method,
							method);
			}
		}

		private static bool CanAccessorRead(PropertyInfo property, AccessorMemberScope scope)
		{
			return scope switch
			{
				AccessorMemberScope.Public => (property.CanRead && property.GetGetMethod(true).IsPublic),
				AccessorMemberScope.All => true,
				AccessorMemberScope.Private => true,
				AccessorMemberScope.None => false,
				_ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
			};
		}

		private static bool CanAccessorWrite(PropertyInfo property, AccessorMemberScope scope)
		{
			return scope switch
			{
				AccessorMemberScope.Public => (property.CanWrite && property.GetSetMethod(true).IsPublic),
				AccessorMemberScope.All => true,
				AccessorMemberScope.Private => true,
				AccessorMemberScope.None => false,
				_ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
			};
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

		private string GetDisplayName()
		{
			var metadata = MetadataType();

			foreach (var attribute in metadata.GetCustomAttributes())
			{
				switch (attribute)
				{
					case DisplayAttribute display:
						return display.Name;
					case DisplayNameAttribute displayName:
						return displayName.DisplayName;
				}
			}

			return DeclaringType.Name;
		}

		private Type MetadataType()
		{
			var metadata = DeclaringType.TryGetAttribute(false, out MetadataTypeAttribute metadataAttribute)
				? metadataAttribute.MetadataType
				: DeclaringType;
			return metadata;
		}
	}
}