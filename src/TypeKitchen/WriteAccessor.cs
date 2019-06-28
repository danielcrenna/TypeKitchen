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

        private static readonly Dictionary<Type, ITypeWriteAccessor> AccessorCache =
            new Dictionary<Type, ITypeWriteAccessor>();

		public static ITypeWriteAccessor Create(object @object, out AccessorMembers members)
		{
			if (@object is Type type)
				return Create(type, out members);
			type = @object.GetType();

			if (!AccessorCache.TryGetValue(type, out var accessor))
				return Create(type, out members);
			members = CreateWriteAccessorMembers(type);
			return accessor;
		}

		public static ITypeWriteAccessor Create(object @object)
		{
			if (@object is Type type)
				return Create(type);
			type = @object.GetType();
			return AccessorCache.TryGetValue(type, out var accessor) ? accessor : Create(type, out _);
		}

		public static ITypeWriteAccessor Create(Type type, out AccessorMembers members)
		{
			if (!AccessorCache.TryGetValue(type, out var accessor))
				return CreateImpl(type, out members);
			members = CreateWriteAccessorMembers(type);
			return accessor;
		}

		public static ITypeWriteAccessor Create(Type type)
		{
			return AccessorCache.TryGetValue(type, out var accessor) ? accessor : Create(type, out _);
		}

		private static ITypeWriteAccessor CreateImpl(Type type, out AccessorMembers members)
		{
			lock (Sync)
			{
				var accessor = CreateWriteAccessor(type, out members);
				AccessorCache[type] = accessor;
				return accessor;
			}
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
                il.Call(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Static | BindingFlags.Public));
                il.Ret();

                var getTypeProperty = tb.DefineProperty(nameof(ITypeWriteAccessor.Type), PropertyAttributes.None,
                    typeof(object), new[] {typeof(string)});
                getTypeProperty.SetGetMethod(getType);

                tb.DefineMethodOverride(getType, typeof(ITypeWriteAccessor).GetMethod($"get_{nameof(ITypeWriteAccessor.Type)}"));
            }

            //
            // bool TryGetValue(object target, string key, out object value):
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
                    il.GotoIfStringEquals(member.Name, branches[member]);	// if (key == "Foo") goto found;
				}

				foreach (var member in members)
				{
					if (!member.CanWrite)
						continue;

                    il.MarkLabel(branches[member]);				// found:
                    il.Ldarg_1();								//     target
                    il.Castclass(type);							//     ({Type}) target
                    il.Ldarg_3();								//     value

                    switch (member.MemberInfo)					//     result = target.{member.Name}
                    {
                        case PropertyInfo property:
	                        if (!property.PropertyType.IsValueType)
		                        il.Castclass(property.PropertyType);
	                        else
		                        il.Unbox_Any(property.PropertyType);
							il.Callvirt(property.GetSetMethod(true));
                            break;
                        case FieldInfo field:
	                        if (!field.FieldType.IsValueType)
		                        il.Castclass(field.FieldType);
	                        else
		                        il.Unbox_Any(field.FieldType);
							il.Stfld(field);
                            break;
                    }
					il.Ldc_I4_1();								//     1
                    il.Ret();									//     return 1  (true)
                }

				il.Ldnull();	//     null
                il.Starg_S();	//     value = null
                il.Ldc_I4_0();	//     0
                il.Ret();		//     return 0 (false)

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

					il.Ldarg_2();											// key
					il.GotoIfStringEquals(member.Name, branches[member]);	// if (key == "Foo") goto found;
				}
				
				foreach (var member in members)
                {
	                if (!member.CanWrite)
		                continue;

					il.MarkLabel(branches[member]); // found:
                    il.Ldarg_1();					//     target
                    il.Castclass(type);				//     ({Type}) target
                    il.Ldarg_3();					//     value

                    switch (member.MemberInfo)		//     result = target.{member.Name}
                    {
                        case PropertyInfo property:
							if(!property.PropertyType.IsValueType)
								il.Castclass(property.PropertyType);
							else
								il.Unbox_Any(property.PropertyType);
                            il.Callvirt(property.GetSetMethod(true));
                            break;
                        case FieldInfo field:
							if (!field.FieldType.IsValueType)
								il.Castclass(field.FieldType);
							else
								il.Unbox_Any(field.FieldType);
							il.Stfld(field);
                            break;
                    }
                    il.Ret();						// return result;
                }

				il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes)).Throw();

                var item = tb.DefineProperty("Item", PropertyAttributes.SpecialName, typeof(object), new[] {typeof(string)});
                item.SetSetMethod(setItem);

                tb.DefineMethodOverride(setItem, typeof(ITypeWriteAccessor).GetMethod("set_Item"));
            }

            var typeInfo = tb.CreateTypeInfo();
            return (ITypeWriteAccessor) Activator.CreateInstance(typeInfo.AsType(), false);
        }

		private static AccessorMembers CreateWriteAccessorMembers(Type type, AccessorMemberScope scope = AccessorMemberScope.All)
		{
			return AccessorMembers.Create(type, scope, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
		}
    }
}