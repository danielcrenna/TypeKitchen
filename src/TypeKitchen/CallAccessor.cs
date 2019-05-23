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
    public static class CallAccessor
    {
        private static readonly object TypeSync = new object();
        private static readonly Dictionary<Type, ITypeCallAccessor> TypeAccessorCache = new Dictionary<Type, ITypeCallAccessor>();

        private static readonly object MethodSync = new object();
        private static readonly Dictionary<MethodBase, IMethodCallAccessor> MethodAccessorCache = new Dictionary<MethodBase, IMethodCallAccessor>();

        public static ITypeCallAccessor Create(object @object)
        {
            if (@object is Type type)
                return Create(type);

            type = @object.GetType();

            lock (TypeSync)
            {
                return TypeAccessorCache.TryGetValue(type, out var accessor) ? accessor : CreateImpl(type);
            }
        }

        public static ITypeCallAccessor Create(Type type)
        {
            lock (TypeSync)
            {
                return TypeAccessorCache.TryGetValue(type, out var accessor) ? accessor : CreateImpl(type);
            }
        }

        private static ITypeCallAccessor CreateImpl(Type type)
        {
            lock (TypeSync)
            {
                var accessor = CreateTypeCallAccessor(type);
                TypeAccessorCache[type] = accessor;
                return accessor;
            }
        }

        public static IMethodCallAccessor Create(MethodInfo methodInfo)
        {
            lock (MethodSync)
            {
                if(MethodAccessorCache.TryGetValue(methodInfo, out var accessor))
                    return accessor;

                accessor = CreateMethodCallAccessor(methodInfo.DeclaringType, methodInfo);
                MethodAccessorCache[methodInfo] = accessor;
                return accessor;
            }
        }

        private static ITypeCallAccessor CreateTypeCallAccessor(Type type, AccessorMemberScope scope = AccessorMemberScope.All)
        {
            var members = AccessorMembers.Create(type, scope, AccessorMemberTypes.Methods);

            var tb = DynamicAssembly.Module.DefineType($"CallAccessor_Type_{type.Assembly.GetHashCode()}_{type.MetadataToken}", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
            tb.AddInterfaceImplementation(typeof(ITypeCallAccessor));

            //
            // Type Type =>:
            //
            tb.MemberProperty(nameof(ITypeCallAccessor.Type), type, typeof(ITypeCallAccessor).GetMethod($"get_{nameof(ITypeCallAccessor.Type)}"));

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
                    if (!member.IsInstanceMethod)
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
                    if (!member.IsInstanceMethod)
                        continue;

                    il.Ldloc_0();
                    il.GotoIfStringEquals(member.Name, branches[member]);
                }

                foreach (var member in members)
                {
                    if (!member.IsInstanceMethod)
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
                        il.Ldc_I4_S((byte)parameters.Length);
                        il.Ldelem_Ref();
                        il.Unbox_Any(parameterTypes[0]);
                    }

                    il.Callvirt(method);
                    il.Nop();

                    il.Ldtoken(typeof(void));
                    il.Call(KnownMethods.GetTypeFromHandle);
                    il.Stloc_1();
                    il.Ldloc_1();
                    il.Ret();
                }
                
                il.Newobj(typeof(ArgumentNullException).GetConstructor(Type.EmptyTypes));
                il.Throw();

                tb.DefineMethodOverride(call, typeof(ITypeCallAccessor).GetMethod(nameof(ITypeCallAccessor.Call)));
            }

            var typeInfo = tb.CreateTypeInfo();
            return (ITypeCallAccessor)Activator.CreateInstance(typeInfo.AsType(), false);
        }

        private static IMethodCallAccessor CreateMethodCallAccessor(Type type, MethodInfo method)
        {
            var tb = DynamicAssembly.Module.DefineType($"CallAccessor_Method_{type.Assembly.GetHashCode()}_{method.MetadataToken}", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass);
            tb.SetParent(typeof(MethodCallAccessor));
            
            var call = tb.DefineMethod(nameof(MethodCallAccessor.Call), MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, typeof(object), new[] { typeof(object), typeof(object[]) });
            call.GetILGeneratorInternal().EmitCall(tb, method);
            tb.DefineMethodOverride(call, KnownMethods.CallWithArgs);

            var constructedType = tb.CreateTypeInfo().AsType();
            var instance = (MethodCallAccessor) Activator.CreateInstance(constructedType);
            instance.MethodName = method.Name;
            instance.Parameters = method.GetParameters();
            return instance;
        }
    }
}