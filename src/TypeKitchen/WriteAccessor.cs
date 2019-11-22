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
				return Create(type, types, scope, out members);
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
					members = CreateWriteAccessorMembers(type, types, scope);
					return accessor;
				}

				accessor = CreateWriteAccessor(type, types, scope, out members);
				AccessorCache[key] = accessor;
				return accessor;
			}
		}

		private static AccessorMembersKey KeyForType(Type type, AccessorMemberTypes types, AccessorMemberScope scope)
		{
			var key = new AccessorMembersKey(type, types, scope);
			return key;
		}

		
		private static ITypeWriteAccessor CreateWriteAccessor(Type type, AccessorMemberTypes types,
			AccessorMemberScope scope, out AccessorMembers members)
		{
			members = CreateWriteAccessorMembers(type, types, scope);

			var name = type.CreateNameForWriteAccessor(members.Types, members.Scope);

			var tb = DynamicAssembly.Module.DefineType(name, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
			tb.AddInterfaceImplementation(typeof(ITypeWriteAccessor));

			//
			// Generate proxies for all backing field setters we will need for this type
			//
			var setFieldDelegateMethods = new Dictionary<MethodBuilder, Delegate>();
			var getFieldDelegateMethods = new List<MethodBuilder>();
			var invokeFieldDelegateMethods = new List<MethodBuilder>();
			var callSwaps = new Dictionary<AccessorMember, MethodInfo>();

			foreach (var member in members)
			{
				if (!member.CanWrite)
					continue; // can't write / is a computed property
				if (!(member.MemberInfo is PropertyInfo property))
					continue; // not a property
				if (property.GetSetMethod(true) != null)
					continue; // has public setter
                
				var backingField = member.BackingField;
				if (backingField == null)
					continue;

				var setFieldMethod = new DynamicMethod($"set_{member.Name}", typeof(void),
					new[] {backingField.DeclaringType, backingField.FieldType}, backingField.DeclaringType, true);

				var setFieldIl = setFieldMethod.GetILGeneratorInternal();
				setFieldIl.Ldarg_0();
				setFieldIl.Ldarg_1();
				setFieldIl.Stfld(backingField);
				setFieldIl.Ret();

				var setFieldDelegateType = typeof(Action<,>).MakeGenericType(backingField.DeclaringType, backingField.FieldType);
				var setFieldDelegate = setFieldMethod.CreateDelegate(setFieldDelegateType);

				// At this point we have a valid delegate that will set the private backing field...

				var setFieldDelegateField = tb.DefineField($"_setFieldDelegate_{member.Type.Namespace}_{member.Type.Name}_{backingField.Name}",
					setFieldDelegateType,
					FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

				var setFieldDelegateMethod = tb.DefineMethod($"_set_setFieldDelegate_{member.Type.Namespace}_{member.Type.Name}_{backingField.Name}",
					MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard,
					typeof(void),
					new[] { setFieldDelegateType });

				var setFieldDelegateIl = setFieldDelegateMethod.GetILGeneratorInternal();
				setFieldDelegateIl.Ldarg_0();
				setFieldDelegateIl.Stsfld(setFieldDelegateField);
				setFieldDelegateIl.Ret();

				setFieldDelegateMethods.Add(setFieldDelegateMethod, setFieldDelegate);

				// At this point we've defined a static field to store our delegate setter and a method to set that field...

				var getFieldDelegateMethod = tb.DefineMethod($"_get_setFieldDelegate_{member.Type.Namespace}_{member.Type.Name}_{backingField.Name}",
					MethodAttributes.Private | MethodAttributes.Static, CallingConventions.Standard,
					setFieldDelegateType, null);

				var getFieldDelegateIl = getFieldDelegateMethod.GetILGeneratorInternal();
				getFieldDelegateIl.Ldsfld(setFieldDelegateField);
				getFieldDelegateIl.Ret();

				getFieldDelegateMethods.Add(getFieldDelegateMethod);

				// At this point we can get and set the static field delegates, and now we need to cache a method to invoke them...
                
				var invokeFieldDelegateMethod = tb.DefineMethod($"_invoke_setFieldDelegate_{member.Type.Namespace}_{member.Type.Name}_{backingField.Name}",
					MethodAttributes.Static | MethodAttributes.Private, CallingConventions.Standard,
					typeof(void),
					new[] { backingField.DeclaringType, backingField.FieldType });

				var invokeFieldDelegateIl = invokeFieldDelegateMethod.GetILGenerator();
				invokeFieldDelegateIl.Emit(OpCodes.Ldsfld, setFieldDelegateField);
				invokeFieldDelegateIl.Emit(OpCodes.Ldarg_0);
				invokeFieldDelegateIl.Emit(OpCodes.Ldarg_1);
				invokeFieldDelegateIl.Emit(OpCodes.Callvirt, setFieldDelegateType.GetMethod("Invoke"));
				invokeFieldDelegateIl.Emit(OpCodes.Ret);

				invokeFieldDelegateMethods.Add(invokeFieldDelegateMethod);
                callSwaps.Add(member, invokeFieldDelegateMethod);
			}

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
				il.CallOrCallvirt(type, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Static | BindingFlags.Public));
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

					il.Ldarg_1();                               // target
					il.CastOrUnbox(type);                       // ({Type}) target
					il.Ldarg_3();                               // value

					switch (member.MemberInfo)      // target.{member.Name} = value
					{
						case PropertyInfo property:
							
							il.CastOrUnboxAny(property.PropertyType);
							il.CallOrCallvirt(type, GetOrSwapPropertySetter(property, callSwaps, member));
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

					il.MarkLabel(branches[member]);     // found:

					il.Ldarg_1();           // target
					il.CastOrUnbox(type);   // ({Type}) target
					il.Ldarg_3();           // value

					switch (member.MemberInfo)			// result = target.{member.Name}
					{
						case PropertyInfo property:
                            il.CastOrUnboxAny(property.PropertyType);
							il.CallOrCallvirt(type, GetOrSwapPropertySetter(property, callSwaps, member));
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

			// Now we need to set the static delegate fields on this type...

			foreach (var entry in setFieldDelegateMethods)
			{
				var setFieldMethod = typeInfo.GetMethod(entry.Key.Name, BindingFlags.Static | BindingFlags.NonPublic);
				if (setFieldMethod == null)
					continue;

				setFieldMethod.Invoke(null, new object[] { entry.Value });
			}

			// Test that we can access the static fields correctly...

			foreach (var entry in getFieldDelegateMethods)
			{
				var getFieldMethod = typeInfo.GetMethod(entry.Name, BindingFlags.Static | BindingFlags.NonPublic);
				if (getFieldMethod == null)
					continue;

				var field = getFieldMethod.Invoke(null, null);
			}

			// Finally, test that we can invoke the delegate and set a backing field

			foreach (var entry in invokeFieldDelegateMethods)
			{
				if (entry.Name != "_invoke_setFieldDelegate_System_String_<Id>k__BackingField")
					continue;

				var invokeFieldMethod = typeInfo.GetMethod(entry.Name, BindingFlags.Static | BindingFlags.NonPublic);
				if (invokeFieldMethod == null)
					throw new NullReferenceException();

				var test = Activator.CreateInstance(type);
				invokeFieldMethod.Invoke(null, new[] { test, "Foo" });
			}

			return (ITypeWriteAccessor) Activator.CreateInstance(typeInfo.AsType(), false);
		}

		private static MethodInfo GetOrSwapPropertySetter(PropertyInfo property, Dictionary<AccessorMember, MethodInfo> callSwaps, AccessorMember member)
		{
			var setMethod = property.GetSetMethod(true);
			if (setMethod == null)
				setMethod = callSwaps[member];
			return setMethod;
		}

		private static AccessorMembers CreateWriteAccessorMembers(Type type, AccessorMemberTypes types, AccessorMemberScope scope)
		{
			return AccessorMembers.Create(type, types, scope);
		}
	}
}