// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TypeKitchen.Internal;

namespace TypeKitchen
{
    public sealed class ReadAccessor
    {
        private static readonly Dictionary<Type, ITypeReadAccessor> AccessorCache = new Dictionary<Type, ITypeReadAccessor>();

        public static ITypeReadAccessor Create(Type type)
        {
            if (AccessorCache.TryGetValue(type, out var accessor))
                return accessor;
            accessor = type.IsAnonymous() ? CreateAnonymousReadAccessor(type) : CreateReadAccessor(type);
            AccessorCache[type] = accessor;
            return accessor;
        }

        private static ITypeReadAccessor CreateReadAccessor(Type type, AccessorMemberScope scope = AccessorMemberScope.All)
        {
            var members = AccessorMembers.Create(type, scope, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);

            var tb = DynamicAssembly.Module.DefineType($"ReadAccessor_{type.Assembly.GetHashCode()}_{type.MetadataToken}",
                TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
            tb.AddInterfaceImplementation(typeof(ITypeReadAccessor));

            //
            // Type Type =>:
            //
            tb.Property(nameof(ITypeReadAccessor.Type), type, typeof(ITypeReadAccessor).GetMethod($"get_{nameof(ITypeReadAccessor.Type)}"));

            //
            // bool TryGetValue(object target, string key, out object value):
            //
            {
                var tryGetValue = tb.DefineMethod(nameof(ITypeReadAccessor.TryGetValue),
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                    MethodAttributes.Virtual | MethodAttributes.NewSlot, typeof(bool),
                    new[] {typeof(object), typeof(string), typeof(object).MakeByRefType()});
                var il = tryGetValue.GetILGeneratorInternal();

                var branches = new Dictionary<AccessorMember, Label>();
                foreach (var member in members)
                    branches.Add(member, il.DefineLabel());

                foreach (var member in members)
                {
                    il.Ldarg_2();                                         // key
                    il.GotoIfStringEquals(member.Name, branches[member]); // if (key == "Foo") goto found;
                }

                foreach (var member in members)
                {
                    il.MarkLabel(branches[member]);                 // found:
                    il.Ldarg_3();                                   //     value
                    il.Ldarg_1();                                   //     target
                    il.Castclass(type);                             //     ({Type}) target
                    switch (member.MemberInfo)                      //     result = target.{member.Name}
                    {
                        case PropertyInfo property:
                            il.Callvirt(property.GetGetMethod());
                            break;
                        case FieldInfo field:
                            il.Ldfld(field);
                            break;
                    }

                    if (member.Type.IsValueType)
                        il.Box(member.Type);                        //     (object) result
                    il.Stind_Ref();                                 //     value = result
                    il.Ldc_I4_1();                                  //     1
                    il.Ret();                                       //     return 1  (true)
                }

                var fail = il.DefineLabel();
                il.Br_S(fail);                                      // goto fail;
                il.MarkLabel(fail);                                 // fail:
                il.Ldarg_3();                                       //     value
                il.Ldnull();                                        //     null
                il.Stind_Ref();                                     //     value = null
                il.Ldc_I4_0();                                      //     0
                il.Ret();                                           //     return 0 (false)

                tb.DefineMethodOverride(tryGetValue, typeof(ITypeReadAccessor).GetMethod("TryGetValue"));
            }

            //
            // object this[object target, string key]:
            //
            {
                var getItem = tb.DefineMethod("get_Item", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(object), new[] {typeof(object), typeof(string)});
                var il = getItem.GetILGeneratorInternal();

                var branches = new Dictionary<AccessorMember, Label>();
                foreach (var member in members)
                    branches.Add(member, il.DefineLabel());

                foreach (var member in members)
                {
                    il.Ldarg_2();                                         // key
                    il.GotoIfStringEquals(member.Name, branches[member]); // if (key == "Foo") goto found;
                }

                foreach (var member in members)
                {
                    il.MarkLabel(branches[member]);
                    il.Ldarg_1();                      // target
                    il.Castclass(type);                // ({Type}) target

                    switch (member.MemberInfo)         // result = target.Foo
                    {
                        case PropertyInfo property:
                            il.Callvirt(property.GetGetMethod());
                            break;
                        case FieldInfo field:
                            il.Ldfld(field);
                            break;
                    }

                    if (member.Type.IsValueType)
                        il.Box(member.Type);          // (object) result
                    il.Ret();                         // return result;
                }

                var fail = il.DefineLabel();
                il.Br_S(fail);
                il.MarkLabel(fail);
                il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes));
                il.Throw();

                var getItemProperty = tb.DefineProperty("Item", PropertyAttributes.SpecialName, typeof(object), new[] {typeof(string)});
                getItemProperty.SetGetMethod(getItem);

                tb.DefineMethodOverride(getItem, typeof(ITypeReadAccessor).GetMethod("get_Item"));
            }

            var typeInfo = tb.CreateTypeInfo();
            return (ITypeReadAccessor) Activator.CreateInstance(typeInfo.AsType(), false);
        }

        /// <summary>
        ///     Anonymous types only have private readonly properties with no logic before their backing fields, so we can do
        ///     a lot to optimize access to them, though we must delegate the access itself due to private reflection rules.
        /// </summary>
        private static ITypeReadAccessor CreateAnonymousReadAccessor(Type type)
        {
            var members = AccessorMembers.Create(type, AccessorMemberScope.Public, AccessorMemberTypes.Properties);

            var tb = DynamicAssembly.Module.DefineType($"ReadAccessor_Anonymous_{type.Assembly.GetHashCode()}_{type.MetadataToken}", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
            tb.AddInterfaceImplementation(typeof(ITypeReadAccessor));

            //
            // Perf: Add static delegates on the type, that store access to the backing fields behind the readonly properties.
            //
            var staticFieldsByMethod = new Dictionary<MethodBuilder, Func<object, object>>();
            var staticFieldsByMember = new Dictionary<AccessorMember, FieldBuilder>();
            foreach (var member in members)
            {
                var backingField = type.GetField($"<{member.Name}>i__Field", BindingFlags.NonPublic | BindingFlags.Instance);
                if (backingField == null)
                    throw new NullReferenceException();

                var dm = new DynamicMethod($"_{member.Name}", typeof(object), new[] {typeof(object)}, tb.Module);
                var dmIl = dm.GetILGenerator();
                dmIl.Emit(OpCodes.Ldarg_0);
                dmIl.Emit(OpCodes.Ldfld, backingField);
                if (backingField.FieldType.IsValueType)
                    dmIl.Emit(OpCodes.Box, backingField.FieldType);
                dmIl.Emit(OpCodes.Ret);
                var backingFieldDelegate = (Func<object, object>) dm.CreateDelegate(typeof(Func<object, object>));

                var getField = tb.DefineField($"_Get{member.Name}", typeof(Func<object, object>), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly);
                var setField = tb.DefineMethod($"_SetGet{member.Name}", MethodAttributes.Static | MethodAttributes.Private, CallingConventions.Standard, typeof(void), new[] {typeof(Func<object, object>)});
                var setFieldIl = setField.GetILGeneratorInternal();
                setFieldIl.Ldarg_0();
                setFieldIl.Stsfld(getField);
                setFieldIl.Ret();

                staticFieldsByMethod.Add(setField, backingFieldDelegate);
                staticFieldsByMember.Add(member, getField);
            }

            //
            // Type Type =>:
            //
            {
                tb.Property(nameof(ITypeReadAccessor.Type), type, typeof(ITypeReadAccessor).GetMethod($"get_{nameof(ITypeReadAccessor.Type)}"));
            }

            //
            // bool TryGetValue(object target, string key, out object value):
            //
            {
                var tryGetValue = tb.DefineMethod(nameof(ITypeReadAccessor.TryGetValue), MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, typeof(bool), new[] {typeof(object), typeof(string), typeof(object).MakeByRefType()});
                var il = tryGetValue.GetILGeneratorInternal();

                var branches = new Dictionary<AccessorMember, Label>();
                foreach (var member in members)
                    branches.Add(member, il.DefineLabel());

                foreach (var member in members)
                {
                    il.Ldarg_2();                                           // key
                    il.GotoIfStringEquals(member.Name, branches[member]);   // if(key == "Foo") goto found;
                }

                foreach (var member in members)
                {
                    var fb = staticFieldsByMember[member];

                    il.MarkLabel(branches[member]);                     // found:
                    il.Ldarg_3();                                       //     value
                    il.Ldsfld(fb);                                      //     _GetFoo
                    il.Ldarg_1();                                       //     target
                    il.Callvirt(fb.FieldType.GetMethod("Invoke"));      //     result = _GetFoo.Invoke(target)
                    if (member.Type.IsValueType) il.Box(member.Type);   //     (object) result
                    il.Stind_Ref();                                     //     value = result
                    il.Ldc_I4_1();                                      //     1
                    il.Ret();                                           //     return 1  (true)
                }

                var fail = il.DefineLabel();
                il.Br_S(fail);                                          // goto fail;
                il.MarkLabel(fail);                                     // fail:
                il.Ldarg_3();                                           //     value
                il.Ldnull();                                            //     null
                il.Stind_Ref();                                         //     value = null
                il.Ldc_I4_0();                                          //     0
                il.Ret();                                               //     return 0 (false)

                tb.DefineMethodOverride(tryGetValue, typeof(ITypeReadAccessor).GetMethod("TryGetValue"));
            }

            //
            // object this[object target, string key]:
            //
            {
                var getItem = tb.DefineMethod("get_Item",
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                    MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(object),
                    new[] {typeof(object), typeof(string)});
                var il = getItem.GetILGeneratorInternal();

                var branches = new Dictionary<AccessorMember, Label>();
                foreach (var member in members)
                    branches.Add(member, il.DefineLabel());

                foreach (var member in members)
                {
                    il.Ldarg_2();                                           // key
                    il.GotoIfStringEquals(member.Name, branches[member]);   // if(key == "Foo") goto found;
                }

                foreach (var member in members)
                {
                    var fb = staticFieldsByMember[member];

                    il.MarkLabel(branches[member]);                    // found:
                    il.Ldsfld(fb);                                     // _GetFoo
                    il.Ldarg_1();                                      // target
                    il.Call(fb.FieldType.GetMethod("Invoke"));         //     result = _GetFoo.Invoke(target)
                    if (member.Type.IsValueType) il.Box( member.Type); //     (object) result
                    il.Ret();                                          // return result;
                }

                var fail = il.DefineLabel();
                il.Br_S(fail);
                il.MarkLabel(fail);
                il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes));
                il.Throw();

                var item = tb.DefineProperty("Item", PropertyAttributes.SpecialName, typeof(object), new[] {typeof(string)});
                item.SetGetMethod(getItem);

                tb.DefineMethodOverride(getItem, typeof(ITypeReadAccessor).GetMethod("get_Item"));
            }

            var typeInfo = tb.CreateTypeInfo();

            //
            // Perf: Set static field values to generated delegate instances.
            //
            foreach (var setter in staticFieldsByMethod)
            {
                var setField = typeInfo.GetMethod(setter.Key.Name, BindingFlags.Static | BindingFlags.NonPublic);
                if (setField == null)
                    throw new NullReferenceException();
                setField.Invoke(null, new object[] {setter.Value});
            }

            return (ITypeReadAccessor) Activator.CreateInstance(typeInfo.AsType(), false);
        }
    }
}