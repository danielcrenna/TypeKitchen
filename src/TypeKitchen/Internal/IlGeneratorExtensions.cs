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

        /// <summary> Creates a property with the specified name and a fixed value, known at runtime.</summary>
        public static void Property(this TypeBuilder tb, string propertyName, MemberInfo value, MethodInfo overrides = null)
        {
            Type type;
            switch (value)
            {
                case Type _:
                    type = typeof(Type);
                    break;
                case MethodInfo _:
                    type = typeof(MethodInfo);
                    break;
                case FieldInfo _:
                    type = typeof(FieldInfo);
                    break;
                case ConstructorInfo _:
                    type = typeof(ConstructorInfo);
                    break;
                default:
                    throw new ArgumentException();
            }

            var getMethod = tb.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, type,
                Type.EmptyTypes);

            if (overrides != null)
                tb.DefineMethodOverride(getMethod, overrides);

            var propertyWithGet = tb.DefineProperty(propertyName, PropertyAttributes.None, typeof(object), Type.EmptyTypes);
            propertyWithGet.SetGetMethod(getMethod);

            var il = getMethod.GetILGeneratorInternal();
            il.Ldtoken(value);
            il.Call(Methods.GetTypeFromHandle);
            il.Ret();
        }
    }
}