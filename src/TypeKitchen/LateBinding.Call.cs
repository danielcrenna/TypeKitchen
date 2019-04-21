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
    partial class LateBinding
    {
        #region DynamicMethod

        public static Dictionary<string, Func<object, object[], object>> DynamicMethodBindCall(AccessorMembers members)
        {
            return members.Members.Where(member => member.CanCall && member.IsInstanceMethod)
                .ToDictionary(member => member.Name, DynamicMethodBindCall);
        }

        public static Func<object, object[], object> DynamicMethodBindCall(AccessorMember member)
        {
            return DynamicMethodBindCall((MethodInfo)member.MemberInfo);
        }

        public static Func<object, object[], object> DynamicMethodBindCall(MethodInfo method)
        {
            var type = method.DeclaringType;
            if (type == null)
                throw new NotSupportedException("Dynamic binding does not currently support anonymous methods");

            var dm = new DynamicMethod($"Call_{method.MetadataToken}", typeof(object), new[] { typeof(object), typeof(object[])});
            dm.GetILGeneratorInternal().EmitCall(type, method);
            return (Func<object, object[], object>) dm.CreateDelegate(typeof(Func<object, object[], object>));
        }

        internal static void EmitCall(this ILSugar il, Type type, MethodInfo method)
        {
            if (!method.IsStatic)
            {
                il.DeclareLocal(type);
                il.Ldarg_0();
                il.Unbox_Any(type);
                il.Stloc_0();
                il.LoadArgument(0);
                if (type.IsValueType)
                    il.Ldloca(0);
                else
                    il.Ldloc_0();
            }
            var parameters = method.GetParameters();
            for (byte i = 0; i < parameters.Length; i++)
            {
                il.Ldarg_1();       // args
                il.LoadConstant(i); // i
                il.Ldelem_Ref();    // args[i]

                var parameterType = parameters[i].ParameterType;
                var byRef = parameterType.IsByRef;
                if (byRef)
                    parameterType = parameterType.GetElementType();

                var arg = il.DeclareLocal(parameterType);
                il.Unbox_Any(parameterType);
                il.Stloc(arg);
                if (byRef)
                    il.Ldloca(arg);
                else
                    il.Ldloc(arg);
            }
            if (method.IsVirtual)
                il.Callvirt(method);
            else
                il.Call(method);
            if (method.IsStatic)
            {
                if (method.ReturnType == typeof(void))
                    il.Ldnull();
                else
                    il.MaybeBox(method.ReturnType);
            }
            il.Ret();
        }

        #endregion
    }
}
