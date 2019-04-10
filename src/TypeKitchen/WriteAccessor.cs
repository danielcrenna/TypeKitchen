﻿// Copyright (c) Blowdart, Inc. All rights reserved.
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
        private static readonly Dictionary<int, ITypeWriteAccessor> AccessorCache = new Dictionary<int, ITypeWriteAccessor>();

        public static ITypeWriteAccessor Create(Type type)
        {
            if (AccessorCache.TryGetValue(type.MetadataToken, out var accessor))
                return accessor;
            accessor = CreateWriteAccessor(type);
            AccessorCache[type.MetadataToken] = accessor;
            return accessor;
        }

        private static ITypeWriteAccessor CreateWriteAccessor(Type type, AccessorMemberScope scope = AccessorMemberScope.All)
        {
            var members = AccessorMembers.Create(type, scope);

            var tb = DynamicAssembly.Module.DefineType($"WriteAccessor_{type.MetadataToken}", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
            tb.AddInterfaceImplementation(typeof(ITypeWriteAccessor));

            //
            // Type Type =>:
            //
            {
                var getType = tb.DefineMethod($"get_{nameof(ITypeWriteAccessor.Type)}",
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                    MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(Type), Type.EmptyTypes);
                var il = getType.GetILGeneratorInternal();
                il.Ldtoken(type);
                il.Call(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Static | BindingFlags.Public));
                il.Ret();

                var getTypeProperty = tb.DefineProperty(nameof(ITypeWriteAccessor.Type), PropertyAttributes.None, typeof(object), new[] { typeof(string) });
                getTypeProperty.SetGetMethod(getType);

                tb.DefineMethodOverride(getType, typeof(ITypeWriteAccessor).GetMethod($"get_{nameof(ITypeWriteAccessor.Type)}"));
            }

            //
            // bool TryGetValue(object target, string key, out object value):
            //
            {
                var trySetValue = tb.DefineMethod(nameof(ITypeWriteAccessor.TrySetValue), MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, typeof(bool), new[] {typeof(object), typeof(string), typeof(object)});
                var il = trySetValue.GetILGeneratorInternal();

                var branches = new Dictionary<AccessorMember, Label>();
                foreach (var member in members)
                    branches.Add(member, il.DefineLabel());

                foreach (var member in members)
                {
                    il.Ldarg_2();                                                                             // key
                    il.Ldstr(member.Name);                                                                    // "Foo"
                    il.Call(typeof(string).GetMethod("op_Equality", new[] {typeof(string), typeof(string)})); // key == "Foo"
                    il.Brtrue_S(branches[member]);                                                            // if(key == "Foo")
                }

                foreach (var member in members)
                {
                    il.MarkLabel(branches[member]);                       // found:
                    il.Ldarg_1();                                         //     target
                    il.Castclass(type);                                   //     ({Type}) target
                    il.Ldarg_3();                                         //     value
                    switch (member.MemberInfo)                            //     result = target.{member.Name}
                    {
                        case PropertyInfo property:
                            il.Castclass(property.PropertyType);          //     ({Type}) value
                            il.Callvirt(property.GetSetMethod());
                            break;
                        case FieldInfo field:
                            il.Castclass(field.FieldType);                //     ({Type}) value
                            il.Stfld(field);
                            break;
                    }

                    if (member.Type.IsValueType)
                        il.Box(member.Type);                              //     (object) result
                    il.Ldc_I4_1();                                        //     1
                    il.Ret();                                             //     return 1  (true)
                }

                var fail = il.DefineLabel();
                il.Br_S(fail);                                            // goto fail;
                il.MarkLabel(fail);                                       // fail:
                il.Ldnull();                                              //     null
                il.Starg_S();                                             //     value = null
                il.Ldc_I4_0();                                            //     0
                il.Ret();                                                 //     return 0 (false)

                tb.DefineMethodOverride(trySetValue,
                    typeof(ITypeWriteAccessor).GetMethod(nameof(ITypeWriteAccessor.TrySetValue)));
            }

            //
            // object this[object target, string key] = object value:
            //
            {
                var setItem = tb.DefineMethod("set_Item", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(void),                     new[] {typeof(object), typeof(string), typeof(object)});
                var il = setItem.GetILGenerator();

                var branches = new Dictionary<AccessorMember, Label>();
                foreach (var member in members)
                    branches.Add(member, il.DefineLabel());

                foreach (var member in members)
                {
                    il.Emit(OpCodes.Ldarg_2);                                                                               // key
                    il.Emit(OpCodes.Ldstr, member.Name);                                                                    // "Foo"
                    il.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new[] {typeof(string), typeof(string)}));
                    il.Emit(OpCodes.Brtrue_S, branches[member]);
                }

                foreach (var member in members)
                {
                    il.MarkLabel(branches[member]);         // found:
                    il.Emit(OpCodes.Ldarg_1);               //     target
                    il.Emit(OpCodes.Castclass, type);       //     ({Type}) target
                    il.Emit(OpCodes.Ldarg_3);               //     value

                    switch (member.MemberInfo)              //     result = target.{member.Name}
                    {
                        case PropertyInfo property:
                            il.Emit(OpCodes.Castclass, property.PropertyType);
                            il.Emit(OpCodes.Callvirt, property.GetSetMethod());
                            break;
                        case FieldInfo field:
                            il.Emit(OpCodes.Castclass, field.FieldType);
                            il.Emit(OpCodes.Stfld, field);
                            break;
                    }

                    if (member.Type.IsValueType)
                        il.Emit(OpCodes.Box, member.Type);  //     (object) result
                    il.Emit(OpCodes.Ret);                   // return result;
                }

                var fail = il.DefineLabel();
                il.Emit(OpCodes.Br_S, fail);
                il.MarkLabel(fail);
                il.Emit(OpCodes.Newobj, typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Throw);

                var item = tb.DefineProperty("Item", PropertyAttributes.SpecialName, typeof(object),
                    new[] {typeof(string)});
                item.SetSetMethod(setItem);

                tb.DefineMethodOverride(setItem, typeof(ITypeWriteAccessor).GetMethod("set_Item"));
            }

            var typeInfo = tb.CreateTypeInfo();
            return (ITypeWriteAccessor) Activator.CreateInstance(typeInfo.AsType(), false);
        }
    }
}