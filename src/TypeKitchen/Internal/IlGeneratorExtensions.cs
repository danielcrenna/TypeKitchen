// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace TypeKitchen.Internal
{
    internal static class IlGeneratorExtensions
    {
        public static ILSugar GetILGeneratorInternal(this MethodBuilder b)
        {
            return new ILSugar(b.GetILGenerator());
        }

        public static ILSugar GotoIfStringEquals(this ILSugar il, string name, Label @goto)
        {
            il.Ldstr(name);
            il.Call(Methods.StringEquals);
            il.Brtrue_S(@goto);
            return il;
        }

        /// <summary> Creates a property with the specified name and fixed value, known at runtime.</summary>
        public static void Property(this TypeBuilder tb, string propertyName, MemberInfo value, MethodInfo overrides = null)
        {
            var getMethod = tb.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, typeof(T),
                Type.EmptyTypes);

            if (overrides != null)
                tb.DefineMethodOverride(getMethod, overrides);

            var propertyWithGet = tb.DefineProperty(propertyName, PropertyAttributes.None, typeof(object), new Type[] { });
            propertyWithGet.SetGetMethod(getMethod);

            var il = getMethod.GetILGeneratorInternal();
            if (value is MemberInfo member)
            {
                il.Ldtoken(member);
                il.Call(Methods.GetTypeFromHandle);
            }
            il.Ret();
        }
    }
}