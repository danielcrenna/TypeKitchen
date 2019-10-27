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
		public AccessorMember(string name, Type type, bool canRead, bool canWrite, bool canCall,
			AccessorMemberScope scope,
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

			if ((info is PropertyInfo || info is FieldInfo) &&
			    Attribute.IsDefined(type, typeof(MetadataTypeAttribute), false))
			{
				var metadata = (MetadataTypeAttribute) Attribute.GetCustomAttribute(type, typeof(MetadataTypeAttribute));

				var surrogate = info switch
				{
					PropertyInfo _ => (MemberInfo) (metadata.MetadataType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new InvalidOperationException()),
					FieldInfo _ => (metadata.MetadataType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new InvalidOperationException()),
					_ => throw new ArgumentException()
				};

				Attributes = Attribute.GetCustomAttributes(surrogate, true);
			}
			else
				Attributes = Attribute.GetCustomAttributes(info, true);

			_display = new Lazy<AccessorMemberDisplay>(() => new AccessorMemberDisplay(this));
		}

		public string Name { get; }
		public Type Type { get; }
		public bool CanRead { get; }
		public bool CanWrite { get; }
		public bool CanCall { get; }
		public AccessorMemberScope Scope { get; }
		public AccessorMemberType MemberType { get; }
		public MemberInfo MemberInfo { get; }
		public Attribute[] Attributes { get; }

		private readonly Lazy<AccessorMemberDisplay> _display;
		public AccessorMemberDisplay Display => _display.Value;


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