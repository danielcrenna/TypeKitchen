// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace TypeKitchen.Internal
{
    internal class ILSugar
    {
        private readonly ILGenerator _il;

        public ILSugar(ILGenerator il)
        {
            _il = il;
        }

        public Label DefineLabel()
        {
            return _il.DefineLabel();
        }

        public ILSugar MarkLabel(Label loc)
        {
            _il.MarkLabel(loc);
            return this;
        }

        #region Correctness Helpers

        public ILSugar Ldarg_0()
        {
            _il.Emit(OpCodes.Ldarg_0);
            return this;
        }

        public ILSugar Ldarg_1()
        {
            _il.Emit(OpCodes.Ldarg_1);
            return this;
        }

        public ILSugar Ldarg_2()
        {
            _il.Emit(OpCodes.Ldarg_2);
            return this;
        }

        public ILSugar Ldarg_3()
        {
            _il.Emit(OpCodes.Ldarg_3);
            return this;
        }

        public ILSugar Ldnull()
        {
            _il.Emit(OpCodes.Ldnull);
            return this;
        }

        public ILSugar Ldstr(string literal)
        {
            _il.Emit(OpCodes.Ldstr, literal);
            return this;
        }

        public ILSugar Ldfld(FieldInfo field)
        {
            _il.Emit(OpCodes.Ldfld, field);
            return this;
        }

        public ILSugar Ldtoken(Type type)
        {
            _il.Emit(OpCodes.Ldtoken, type);
            return this;
        }

        public ILSugar Ldsfld(FieldInfo field)
        {
            _il.Emit(OpCodes.Ldsfld, field);
            return this;
        }

        public ILSugar Stfld(FieldInfo field)
        {
            _il.Emit(OpCodes.Stfld, field);
            return this;
        }

        public ILSugar Stsfld(FieldInfo field)
        {
            _il.Emit(OpCodes.Stsfld, field);
            return this;
        }

        public ILSugar Starg_S()
        {
            _il.Emit(OpCodes.Starg_S);
            return this;
        }

        public ILSugar Call(MethodInfo method)
        {
            return CallOrCallvirt(method);
        }

        public ILSugar Callvirt(MethodInfo method)
        {
            return CallOrCallvirt(method);
        }

        private ILSugar CallOrCallvirt(MethodInfo method)
        {
            _il.Emit(method.IsFinal || !method.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, method);
            return this;
        }

        public ILSugar Castclass(Type type)
        {
            _il.Emit(OpCodes.Castclass, type);
            return this;
        }

        public ILSugar Br_S(Label label)
        {
            _il.Emit(OpCodes.Br_S, label);
            return this;
        }

        public ILSugar Newobj(ConstructorInfo ctor)
        {
            _il.Emit(OpCodes.Newobj, ctor);
            return this;
        }

        public ILSugar Throw()
        {
            _il.Emit(OpCodes.Throw);
            return this;
        }

        public ILSugar Brtrue_S(Label label)
        {
            _il.Emit(OpCodes.Brtrue_S, label);
            return this;
        }

        public ILSugar Box(Type type)
        {
            _il.Emit(OpCodes.Box, type);
            return this;
        }

        public ILSugar Ret()
        {
            _il.Emit(OpCodes.Ret);
            return this;
        }

        public ILSugar Stind_Ref()
        {
            _il.Emit(OpCodes.Stind_Ref);
            return this;
        }

        public ILSugar Ldc_I4_0()
        {
            _il.Emit(OpCodes.Ldc_I4_0);
            return this;
        }

        public ILSugar Ldc_I4_1()
        {
            _il.Emit(OpCodes.Ldc_I4_1);
            return this;
        }

        #endregion
    }
}