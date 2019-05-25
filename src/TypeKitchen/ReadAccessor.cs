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
        private static readonly object Sync = new object();
        private static readonly Dictionary<Type, ITypeReadAccessor> AccessorCache = new Dictionary<Type, ITypeReadAccessor>();

        public static ITypeReadAccessor Create(object @object, out AccessorMembers members)
        {
            if (@object is Type type)
                return Create(type, out members);
            type = @object.GetType();

            if (!AccessorCache.TryGetValue(type, out var accessor))
                return Create(type, @object, out members);
            members = type.IsAnonymous() ? CreateAnonymousReadAccessorMembers(type) : CreateReadAccessorMembers(type);
            return accessor;
        }

        public static ITypeReadAccessor Create(object @object)
        {
            if (@object is Type type)
                return Create(type);
            type = @object.GetType();
            return AccessorCache.TryGetValue(type, out var accessor) ? accessor : Create(type, @object, out _);
        }

        public static ITypeReadAccessor Create(Type type, out AccessorMembers members)
        {
            if (!AccessorCache.TryGetValue(type, out var accessor))
                return Create(type, null, out members);
            members = type.IsAnonymous() ? CreateAnonymousReadAccessorMembers(type) : CreateReadAccessorMembers(type);
            return accessor;
        }

        public static ITypeReadAccessor Create(Type type)
        {
            return AccessorCache.TryGetValue(type, out var accessor) ? accessor : Create(type, null, out _);
        }
        
        private static ITypeReadAccessor Create(Type type, object @object, out AccessorMembers members)
        {
            lock (Sync)
            {
                var accessor = type.IsAnonymous()
                    ? CreateAnonymousReadAccessor(type, out members, @object)
                    : CreateReadAccessor(type, out members);
                AccessorCache[type] = accessor;
                return accessor;
            }
        }

        private static ITypeReadAccessor CreateReadAccessor(Type type, out AccessorMembers members, AccessorMemberScope scope = AccessorMemberScope.All)
        {
            members = CreateReadAccessorMembers(type, scope);

            var tb = DynamicAssembly.Module.DefineType($"ReadAccessor_{type.Assembly.GetHashCode()}_{type.MetadataToken}",
                TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
            tb.AddInterfaceImplementation(typeof(ITypeReadAccessor));

            //
            // Type Type =>:
            //
            tb.MemberProperty(nameof(ITypeReadAccessor.Type), type, typeof(ITypeReadAccessor).GetMethod($"get_{nameof(ITypeReadAccessor.Type)}"));

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
                var fail = il.DefineLabel();

                foreach (var member in members)
                {
                    il.Ldarg_2();                                         // key
                    il.GotoIfStringEquals(member.Name, branches[member]); // if (key == "Foo") goto found;
                }

                il.Br(fail);

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

                il.MarkLabel(fail);
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
                
                il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes));
                il.Throw();

                var getItemProperty = tb.DefineProperty("Item", PropertyAttributes.SpecialName, typeof(object), new[] {typeof(string)});
                getItemProperty.SetGetMethod(getItem);

                tb.DefineMethodOverride(getItem, typeof(ITypeReadAccessor).GetMethod("get_Item"));
            }

            var typeInfo = tb.CreateTypeInfo();
            return (ITypeReadAccessor) Activator.CreateInstance(typeInfo.AsType(), false);
        }

        private static AccessorMembers CreateReadAccessorMembers(Type type, AccessorMemberScope scope = AccessorMemberScope.All) => AccessorMembers.Create(type, scope, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);

        private static AccessorMembers CreateAnonymousReadAccessorMembers(Type type) => AccessorMembers.Create(type, AccessorMemberScope.Public, AccessorMemberTypes.Properties);

        /// <summary>
        ///     Anonymous types only have private readonly properties with no logic before their backing fields, so we can do
        ///     a lot to optimize access to them, though we must delegate the access itself due to private reflection rules.
        /// </summary>
        private static ITypeReadAccessor CreateAnonymousReadAccessor(Type type, out AccessorMembers members, object debugObject = null)
        {
            members = CreateAnonymousReadAccessorMembers(type);

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
                var dmIl = dm.GetILGeneratorInternal();
                dmIl.Ldarg_0()
                    .Ldfld(backingField);
                if (backingField.FieldType.IsValueType)
                    dmIl.Box(backingField.FieldType);
                dmIl.Ret();
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
                tb.MemberProperty(nameof(ITypeReadAccessor.Type), type, typeof(ITypeReadAccessor).GetMethod($"get_{nameof(ITypeReadAccessor.Type)}"));
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
                var fail = il.DefineLabel();

                foreach (var member in members)
                {
                    il.Ldarg_2();                                           // key
                    il.GotoIfStringEquals(member.Name, branches[member]);   // if(key == "Foo") goto found;
                }

                il.Br(fail);

                foreach (var member in members)
                {
                    var fb = staticFieldsByMember[member];

                    il.MarkLabel(branches[member]);                     // found:
                    il.Ldarg_3();                                       //     value
                    il.Ldsfld(fb);                                      //     _GetFoo
                    il.Ldarg_1();                                       //     target
                    il.Call(fb.FieldType.GetMethod("Invoke"));          //     result = _GetFoo.Invoke(target)
                    il.Stind_Ref();                                     //     value = result
                    il.Ldc_I4_1();                                      //     1
                    il.Ret();                                           //     return 1 (true)
                }

                il.MarkLabel(fail);
                il.Ldarg_3();                                           //     value
                il.Ldnull();                                            //     null
                il.Stind_Ref();                                         //     value = null
                il.Ldc_I4_0();                                          //     0
                il.Ret();                                               //     return 0 (false)

                tb.DefineMethodOverride(tryGetValue, typeof(ITypeReadAccessor).GetMethod(nameof(ITypeReadAccessor.TryGetValue)));
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
                    il.Ret();                                          // return result;
                }

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
                setField.Invoke(null, new object[] { setter.Value });

                if (debugObject != null)
                {
                    var memberName = setter.Key.Name.Replace("_SetGet", string.Empty);

                    var staticFieldFunc = (Func<object, object>)typeInfo.GetField($"_Get{memberName}").GetValue(debugObject);
                    if (staticFieldFunc != setter.Value)
                        throw new ArgumentException($"replacing _Get{memberName} with function from _SetGet{memberName} was unsuccessful");

                    var backingField = type.GetField($"<{memberName}>i__Field", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (backingField == null)
                        throw new NullReferenceException("backing field was not found");

                    var backingFieldValue = backingField.GetValue(debugObject);
                    var cachedDelegateValue = setter.Value(debugObject);
                    if (!backingFieldValue.Equals(cachedDelegateValue))
                        throw new ArgumentException($"{memberName} backing field value '{backingFieldValue}' does not agree with cached delegate value {cachedDelegateValue}");
                }
            }

            var accessor = (ITypeReadAccessor) Activator.CreateInstance(typeInfo, false);

            if (debugObject != null)
            {
                foreach (var member in members)
                {
                    var byAccessor = accessor[debugObject, member.Name];
                    var byReflection = ((Func<object, object>)typeInfo.GetField($"_Get{member.Name}").GetValue(debugObject))(debugObject);
                    if(!byAccessor.Equals(byReflection))
                        throw new InvalidOperationException("IL produced incorrect accessor");
                }
            }

            return accessor;
        }
    }
}