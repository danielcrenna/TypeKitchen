// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TypeKitchen.Internal;

namespace TypeKitchen
{
	public sealed class WriteAccessor
	{
		private static readonly object Sync = new object();

		private static readonly Dictionary<AccessorMembersKey, ITypeWriteAccessor> AccessorCache =
			new Dictionary<AccessorMembersKey, ITypeWriteAccessor>();

		public static ITypeWriteAccessor Create(object @object, out AccessorMembers members)
		{
			return Create(@object, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.All,
				out members);
		}

		public static ITypeWriteAccessor Create(object @object, AccessorMemberTypes types, out AccessorMembers members)
		{
			return Create(@object, types, AccessorMemberScope.All,
				out members);
		}

		public static ITypeWriteAccessor Create(object @object, AccessorMemberScope scope, out AccessorMembers members)
		{
			return Create(@object, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, scope,
				out members);
		}

		public static ITypeWriteAccessor Create(object @object, AccessorMemberTypes types, AccessorMemberScope scope,
			out AccessorMembers members)
		{
			if (@object is Type type)
				return Create(type, out members);
			type = @object.GetType();
			return Create(type, types, scope, out members);
		}

		public static ITypeWriteAccessor Create(object @object,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			if (@object is Type type)
				return Create(type, types, scope);
			type = @object.GetType();
			return Create(type, types, scope, out _);
		}

		public static ITypeWriteAccessor Create(Type type, AccessorMemberTypes types, out AccessorMembers members)
		{
			return Create(type, types, AccessorMemberScope.All, out members);
		}

		public static ITypeWriteAccessor Create(Type type, AccessorMemberScope scope, out AccessorMembers members)
		{
			return Create(type, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, scope,
				out members);
		}

		public static ITypeWriteAccessor Create(Type type, out AccessorMembers members)
		{
			return Create(type, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.All,
				out members);
		}

		public static ITypeWriteAccessor Create(Type type, AccessorMemberTypes types, AccessorMemberScope scope,
			out AccessorMembers members)
		{
			return CreateImpl(type, types, scope, out members);
		}

		public static ITypeWriteAccessor Create(Type type,
			AccessorMemberTypes types = AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			return CreateImpl(type, types, scope, out _);
		}

		private static ITypeWriteAccessor CreateImpl(Type type, AccessorMemberTypes types, AccessorMemberScope scope,
			out AccessorMembers members)
		{
			lock (Sync)
			{
				var key = KeyForType(type, types, scope);

				if (AccessorCache.TryGetValue(key, out var accessor))
				{
					members = CreateWriteAccessorMembers(type, scope);
					return accessor;
				}

				accessor = CreateWriteAccessor(type, out members);
				AccessorCache[key] = accessor;
				return accessor;
			}
		}

		private static AccessorMembersKey KeyForType(Type type, AccessorMemberTypes types, AccessorMemberScope scope)
		{
			var key = new AccessorMembersKey(type, types, scope);
			return key;
		}

		private static ITypeWriteAccessor CreateWriteAccessor(Type type, out AccessorMembers members,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			members = CreateWriteAccessorMembers(type, scope);

			var tb = DynamicAssembly.Module.DefineType(
				$"WriteAccessor_{(type.Assembly.IsDynamic ? "DynamicAssembly" : type.Assembly.GetName().Name)}_{type.FullName}",
				TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit |
				TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
			tb.AddInterfaceImplementation(typeof(ITypeWriteAccessor));

			//
			// Type Type =>:
			//
			{
				var getType = tb.DefineMethod($"get_{nameof(ITypeWriteAccessor.Type)}",
					MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
					MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(Type),
					Type.EmptyTypes);
				var il = getType.GetILGeneratorInternal();
				il.Ldtoken(type);
				il.CallOrCallvirt(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Static | BindingFlags.Public), type);
				il.Ret();

				var getTypeProperty = tb.DefineProperty(nameof(ITypeWriteAccessor.Type), PropertyAttributes.None,
					typeof(object), new[] {typeof(string)});
				getTypeProperty.SetGetMethod(getType);

				tb.DefineMethodOverride(getType,
					typeof(ITypeWriteAccessor).GetMethod($"get_{nameof(ITypeWriteAccessor.Type)}"));
			}

			//
			// bool TrySetValue(object target, string key, object value):
			//
			{
				var trySetValue = tb.DefineMethod(nameof(ITypeWriteAccessor.TrySetValue),
					MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
					MethodAttributes.Virtual | MethodAttributes.NewSlot, typeof(bool),
					new[] {typeof(object), typeof(string), typeof(object)});
				var il = trySetValue.GetILGeneratorInternal();

				var branches = new Dictionary<AccessorMember, Label>();
				foreach (var member in members)
				{
					if (!member.CanWrite)
						continue;
					branches.Add(member, il.DefineLabel());
				}

				foreach (var member in members)
				{
					if (!member.CanWrite)
						continue;

					il.Ldarg_2();											// key
					il.GotoIfStringEquals(member.Name, branches[member]);   // if (key == "{member.Name}") goto found;
				}

				foreach (var member in members)
				{
					if (!member.CanWrite)
						continue;

					il.MarkLabel(branches[member]); // found:
					il.Ldarg_1();					// target
					il.CastOrUnbox(type);			// ({Type}) target

					il.Ldarg_3();                   // value

					switch (member.MemberInfo)      // target.{member.Name} = value
					{
						case PropertyInfo property:
							il.CastOrUnboxAny(property.PropertyType);
							il.CallOrCallvirt(property.GetSetMethod(true), type);
							break;
						case FieldInfo field:
							il.CastOrUnboxAny(field.FieldType);
							il.Stfld(field);
							break;
					}
					
					il.Ldc_I4_1();					//     1
					il.Ret();						//     return 1 (true)
				}

				il.Ldnull();						//     null
				il.Starg_S();						//     value = null
				il.Ldc_I4_0();						//     0
				il.Ret();							//     return 0 (false)

				tb.DefineMethodOverride(trySetValue,
					typeof(ITypeWriteAccessor).GetMethod(nameof(ITypeWriteAccessor.TrySetValue)));
			}

			//
			// object this[object target, string key] = object value:
			//
			{
				var setItem = tb.DefineMethod("set_Item",
					MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
					MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(void),
					new[] {typeof(object), typeof(string), typeof(object)});
				var il = setItem.GetILGeneratorInternal();

				var branches = new Dictionary<AccessorMember, Label>();
				foreach (var member in members)
				{
					if (!member.CanWrite)
						continue;
					branches.Add(member, il.DefineLabel());
				}

				foreach (var member in members)
				{
					if (!member.CanWrite)
						continue;

					il.Ldarg_2(); // key
					il.GotoIfStringEquals(member.Name, branches[member]); // if (key == "{member.Name}") goto found;
				}

				foreach (var member in members)
				{
					if (!member.CanWrite)
						continue;

					il.MarkLabel(branches[member]);		// found:
					il.Ldarg_1();						// target
					il.CastOrUnbox(type);				// ({Type}) target
					il.Ldarg_3();						// value

					switch (member.MemberInfo)			// result = target.{member.Name}
					{
						case PropertyInfo property:
							il.CastOrUnboxAny(property.PropertyType);
							il.CallOrCallvirt(property.GetSetMethod(true), type);
							break;
						case FieldInfo field:
							il.CastOrUnboxAny(field.FieldType);
							il.Stfld(field);
							break;
					}

					il.Ret();							// return result;
				}

				il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes)).Throw();

				var item = tb.DefineProperty("Item", PropertyAttributes.SpecialName, typeof(object),
					new[] {typeof(string)});
				item.SetSetMethod(setItem);

				tb.DefineMethodOverride(setItem, typeof(ITypeWriteAccessor).GetMethod("set_Item"));
			}

			var typeInfo = tb.CreateTypeInfo();
			return (ITypeWriteAccessor) Activator.CreateInstance(typeInfo.AsType(), false);
		}

		private static AccessorMembers CreateWriteAccessorMembers(Type type,
			AccessorMemberScope scope = AccessorMemberScope.All)
		{
			return AccessorMembers.Create(type, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, scope);
		}
	}
}