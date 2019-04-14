// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TypeKitchen.Internal;

namespace TypeKitchen
{
    public sealed class CallAccessor
    {
        private static readonly Dictionary<int, ITypeCallAccessor> TypeAccessorCache = new Dictionary<int, ITypeCallAccessor>();
        private static readonly Dictionary<int, IMethodCallAccessor> MethodAccessorCache = new Dictionary<int, IMethodCallAccessor>();

        public static ITypeCallAccessor Create(Type type)
        {
            if (TypeAccessorCache.TryGetValue(type.MetadataToken, out var accessor))
                return accessor;
            accessor = CreateCallAccessor(type);
            TypeAccessorCache[type.MetadataToken] = accessor;
            return accessor;
        }

        private static ITypeCallAccessor CreateCallAccessor(Type type, AccessorMemberScope scope = AccessorMemberScope.All)
        {
            var members = AccessorMembers.Create(type, scope, AccessorMemberTypes.Methods);

            var tb = DynamicAssembly.Module.DefineType($"CallAccessor_Type_{type.MetadataToken}", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
            tb.AddInterfaceImplementation(typeof(ITypeCallAccessor));

            //
            // Type Type =>:
            //
            tb.Property(nameof(ITypeCallAccessor.Type), type, typeof(ITypeCallAccessor).GetMethod($"get_{nameof(ITypeCallAccessor.Type)}"));

            //
            // object Call(object target, string name, params object[] args):
            //
            {
                var call = tb.DefineMethod(nameof(ITypeCallAccessor.Call),
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                    MethodAttributes.Virtual | MethodAttributes.NewSlot,
                    typeof(object), new[] { typeof(object), typeof(string), typeof(object[]) });

                var il = call.GetILGeneratorInternal();

                var branches = new Dictionary<AccessorMember, Label>();
                foreach (var member in members)
                {
                    if (!IsInstanceMethod(member))
                        continue;

                    branches.Add(member, il.DefineLabel());
                }

                il.DeclareLocal(typeof(string));
                il.DeclareLocal(typeof(object));
                il.Nop();
                il.Ldarg_2();
                il.Stloc_0();

                foreach (var member in members)
                {
                    if (!IsInstanceMethod(member))
                        continue;

                    il.Ldloc_0();
                    il.GotoIfStringEquals(member.Name, branches[member]);
                }

                foreach (var member in members)
                {
                    if (!IsInstanceMethod(member))
                        continue;

                    var method = (MethodInfo)member.MemberInfo;
                    var parameters = method.GetParameters();
                    var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

                    il.MarkLabel(branches[member]);
                    il.Ldarg_1();
                    il.Castclass(method.DeclaringType);

                    var returns = method.ReturnType != typeof(void);
                    if (returns)
                    {
                        continue;
                        throw new NotImplementedException();
                    }

                    if (parameters.Length > 0)
                    {
                        il.Ldarg_3();
                        il.Ldc_I4_S((byte) parameters.Length);
                        il.Ldelem_Ref();
                        il.Unbox_Any(parameterTypes[0]);
                    }

                    il.Callvirt(method);
                    il.Nop();

                    il.Ldtoken(typeof(void));
                    il.Call(Methods.GetTypeFromHandle);
                    il.Stloc_1();
                    il.Ldloc_1();
                    il.Ret();
                }

                var fail = il.DefineLabel();
                il.Br_S(fail);
                il.MarkLabel(fail);
                il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes));
                il.Throw();

                tb.DefineMethodOverride(call, typeof(ITypeCallAccessor).GetMethod(nameof(ITypeCallAccessor.Call)));
            }

            var typeInfo = tb.CreateTypeInfo();
            return (ITypeCallAccessor)Activator.CreateInstance(typeInfo.AsType(), false);

            bool IsInstanceMethod(AccessorMember member)
            {
                return member.MemberInfo is MethodInfo method &&
                       !method.Name.StartsWith("get_") &&
                       !method.Name.StartsWith("set_") &&
                       method.DeclaringType != typeof(object);
            }
        }

        public static IMethodCallAccessor Create(MethodInfo methodInfo)
        {
            if (MethodAccessorCache.TryGetValue(methodInfo.MetadataToken, out var accessor))
                return accessor;
            accessor = CreateCallAccessor(methodInfo);
            MethodAccessorCache[methodInfo.MetadataToken] = accessor;
            return accessor;
        }

        private static IMethodCallAccessor CreateCallAccessor(MethodInfo method)
        {
            var tb = DynamicAssembly.Module.DefineType($"CallAccessor_Method_{method.MetadataToken}", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
            tb.AddInterfaceImplementation(typeof(IMethodCallAccessor));

            //
            // MethodInfo MethodInfo =>:
            //
            tb.Property(nameof(IMethodCallAccessor.MethodInfo), method, typeof(IMethodCallAccessor).GetMethod($"get_{nameof(IMethodCallAccessor.MethodInfo)}"));

            //
            // object Call(object target, params object[] args):
            //
            {
                var call = tb.DefineMethod(nameof(IMethodCallAccessor.Call),
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                    MethodAttributes.Virtual | MethodAttributes.NewSlot,
                    typeof(object), new[] { typeof(object), typeof(object[]) });

                var parameters = method.GetParameters();
                var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

                var il = call.GetILGeneratorInternal();
                il.DeclareLocal(typeof(object));
                il.Nop();
                il.Ldarg_1();
                il.Castclass(method.DeclaringType);
                il.Callvirt(method);
                il.Nop();
                il.Ldtoken(method.ReturnType);
                il.Call(Methods.GetTypeFromHandle);
                il.Stloc_0();
                il.Ldloc_0();
                il.Ret();

                tb.DefineMethodOverride(call, typeof(IMethodCallAccessor).GetMethod(nameof(IMethodCallAccessor.Call)));
            }

            var typeInfo = tb.CreateTypeInfo();
            return (IMethodCallAccessor)Activator.CreateInstance(typeInfo.AsType(), false);
        }
    }
}