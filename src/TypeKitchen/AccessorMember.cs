// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace TypeKitchen
{
	[DebuggerDisplay("{" + nameof(MemberInfo) + "}")]
	public sealed class AccessorMember
	{
		internal AccessorMember(Type declaringType, string name, Type type, bool canRead, bool canWrite, bool canCall, AccessorMemberScope scope, AccessorMemberType memberType, MemberInfo memberInfo)
		{
			DeclaringType = declaringType;
			Name = name;
			Type = type;
			CanRead = canRead;
			_canWrite = canWrite;
			CanCall = canCall;
			Scope = scope;
			MemberType = memberType;
			MemberInfo = memberInfo;

			if (Attribute.IsDefined(type, typeof(MetadataTypeAttribute), false))
			{
				SetAttributesFromSurrogate(type, memberInfo);
			}
			if ((memberInfo is PropertyInfo || memberInfo is FieldInfo) && Attribute.IsDefined(memberInfo, typeof(MetadataTypeAttribute), false))
			{
				SetAttributesFromSurrogate(memberInfo, memberInfo);
			}
			else
			{
				Attributes = Attribute.GetCustomAttributes(memberInfo, true);
			}

			_displayMap = new Dictionary<string, Lazy<AccessorMemberDisplay>>();
		}

		private void SetAttributesFromSurrogate(MemberInfo authority, MemberInfo memberInfo)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var metadata = (MetadataTypeAttribute) Attribute.GetCustomAttribute(authority, typeof(MetadataTypeAttribute));
			var surrogate = memberInfo switch
			{
				PropertyInfo _ => (MemberInfo) (metadata.MetadataType.GetProperty(memberInfo.Name, flags) ?? throw new InvalidOperationException()), 
				FieldInfo _ => (metadata.MetadataType.GetField(memberInfo.Name, flags) ?? throw new InvalidOperationException()),
				_ => throw new ArgumentException()
			};
			Attributes = Attribute.GetCustomAttributes(surrogate, true);
		}

		public bool IsComputedProperty => MemberInfo is PropertyInfo p && p.GetSetMethod(true) == null && BackingField == null;
		public FieldInfo BackingField => DeclaringType.GetField($"<{Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);

		public string Name { get; }
		public Type DeclaringType { get; }
		public Type Type { get; }

		public bool CanRead { get; }
		public bool CanWrite => _canWrite && !IsComputedProperty;

		public bool CanCall { get; }
		public AccessorMemberScope Scope { get; }
		public AccessorMemberType MemberType { get; }
		public MemberInfo MemberInfo { get; }
		public Attribute[] Attributes { get; private set; }

		private readonly Dictionary<string, Lazy<AccessorMemberDisplay>> _displayMap;
		private readonly bool _canWrite;

		private Lazy<AccessorMemberDisplay> AddDisplayProfile(string profile)
		{
			var lazy = new Lazy<AccessorMemberDisplay>(() => new AccessorMemberDisplay(this, profile));
			_displayMap.Add(profile, lazy);
			return lazy;
		}

		public AccessorMemberDisplay Display(string profile)
		{
			if(!_displayMap.TryGetValue(profile, out var lazy))
				lazy = AddDisplayProfile(profile);

			return lazy.Value;
		}

		internal bool IsInstanceMethod => CanCall && MemberInfo is MethodInfo method &&
		                                  !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") &&
		                                  method.DeclaringType != typeof(object);

		public bool HasAttribute<T>() where T : Attribute
		{
			foreach (var attr in Attributes)
				if (attr is T)
					return true;
			return false;
		}

		public bool TryGetAttribute<T>(out T attribute) where T : Attribute
		{
			foreach (var attr in Attributes)
			{
				if (!(attr is T a))
					continue;
				attribute = a;
				return true;
			}

			attribute = default;
			return false;
		}

		public bool TryGetAttributes<T>(out Attribute[] attributes) where T : Attribute
		{
			var capacity = 0;
			foreach (var attr in Attributes)
			{
				if (!(attr is T))
					continue;
				capacity++;
			}

			if (capacity == 0)
			{
				attributes = default;
				return false;
			}

			attributes = new Attribute[capacity];
			for (var i = 0; i < Attributes.Length; i++)
			{
				var attr = Attributes[i];
				if (!(attr is T a))
					continue;
				attributes[i] = a;
			}

			return true;
		}

		public IEnumerable<T> GetAttributes<T>(bool canInherit = true) where T : Attribute
		{
			return Attribute.GetCustomAttributes(MemberInfo, typeof(T), canInherit).Cast<T>();
		}
	}
}