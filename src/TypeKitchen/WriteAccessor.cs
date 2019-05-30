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

        public static ITypeWriteAccessor Create(object @object)
        {
            if (@object is Type type)
                return Create(type);

            type = @object.GetType();

            return AccessorCache.TryGetValue(type, out var accessor) ? accessor : CreateImpl(type);
        }

        public static ITypeWriteAccessor Create(Type type)
        {
            return AccessorCache.TryGetValue(type, out var accessor) ? accessor : CreateImpl(type);
        }

        private static ITypeWriteAccessor CreateImpl(Type type)
        {
            lock (Sync)
            {
                var accessor = CreateWriteAccessor(type);
                AccessorCache[type] = accessor;
                return accessor;
            }
        }

        private static ITypeWriteAccessor CreateWriteAccessor(Type type,
            AccessorMemberScope scope = AccessorMemberScope.All)
        {
            var members =
                AccessorMembers.Create(type, scope, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);

            var tb = DynamicAssembly.Module.DefineType(
                $"WriteAccessor_{type.Assembly.GetHashCode()}_{type.MetadataToken}",
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
                il.Call(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle),
                    BindingFlags.Static | BindingFlags.Public));
                il.Ret();

                var getTypeProperty = tb.DefineProperty(nameof(ITypeWriteAccessor.Type), PropertyAttributes.None,
                    typeof(object), new[] {typeof(string)});
                getTypeProperty.SetGetMethod(getType);

                tb.DefineMethodOverride(getType,
                    typeof(ITypeWriteAccessor).GetMethod($"get_{nameof(ITypeWriteAccessor.Type)}"));
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
                    branches.Add(member, il.DefineLabel());

                foreach (var member in members)
                {
                    il.Ldarg_2(); // key
                    il.Ldstr(member.Name); // "Foo"
                    il.Call(KnownMethods.StringEquals); // key == "Foo"
                    il.Brtrue(branches[member]); // if(key == "Foo")
                }

                foreach (var member in members)
                {
                    il.MarkLabel(branches[member]); // found:
                    il.Ldarg_1(); //     target
                    il.Castclass(type); //     ({Type}) target
                    il.Ldarg_3(); //     value
                    switch (member.MemberInfo) //     result = target.{member.Name}
                    {
                        case PropertyInfo property:
                            il.Castclass(property.PropertyType); //     ({Type}) value
                            il.Callvirt(property.GetSetMethod());
                            break;
                        case FieldInfo field:
                            il.Castclass(field.FieldType); //     ({Type}) value
                            il.Stfld(field);
                            break;
                    }

                    if (member.Type.IsValueType)
                        il.Box(member.Type); //     (object) result
                    il.Ldc_I4_1(); //     1
                    il.Ret(); //     return 1  (true)
                }

                il.Ldnull(); //     null
                il.Starg_S(); //     value = null
                il.Ldc_I4_0(); //     0
                il.Ret(); //     return 0 (false)

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
                    branches.Add(member, il.DefineLabel());

                foreach (var member in members)
                {
                    il.Ldarg_2(); // key
                    il.Ldstr(member.Name); // "Foo"
                    il.Call(KnownMethods.StringEquals);
                    il.Brtrue(branches[member]);
                }

                foreach (var member in members)
                {
                    il.MarkLabel(branches[member]); // found:
                    il.Ldarg_1(); //     target
                    il.Castclass(type); //     ({Type}) target
                    il.Ldarg_3(); //     value

                    switch (member.MemberInfo) //     result = target.{member.Name}
                    {
                        case PropertyInfo property:
                            il.Castclass(property.PropertyType);
                            il.Callvirt(property.GetSetMethod());
                            break;
                        case FieldInfo field:
                            il.Castclass(field.FieldType);
                            il.Stfld(field);
                            break;
                    }

                    if (member.Type.IsValueType)
                        il.Box(member.Type); //     (object) result
                    il.Ret(); // return result;
                }

                il
                    .Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes))
                    .Throw();

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