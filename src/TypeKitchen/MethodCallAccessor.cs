// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Reflection;

namespace TypeKitchen
{
    public abstract class MethodCallAccessor : IMethodCallAccessor
    {
        public string MethodName { get; set; }
        public ParameterInfo[] Parameters { get; set; }

        private static readonly object[] NoArgs = { };
        private static readonly ArrayPool<object> Arguments = ArrayPool<object>.Create();

        public object Call(object target)
        {
            return Call(target, NoArgs);
        }

        public object Call(object target, object arg1)
        {
            var args = RentAndResize(1);
            args[0] = arg1;
            try
            {
                return Call(target, args);
            }
            finally
            {
                Arguments.Return(args, true);
            }
        }

        public object Call(object target, object arg1, object arg2)
        {
            var args = RentAndResize(2);
            args[0] = arg1;
            args[1] = arg2;
            try
            {
                return Call(target, args);
            }
            finally
            {
                Arguments.Return(args, true);
            }
        }

        public object Call(object target, object arg1, object arg2, object arg3)
        {
            var args = RentAndResize(3);
            args[0] = arg1;
            args[1] = arg2;
            args[2] = arg3;
            try
            {
                return Call(target, args);
            }
            finally
            {
                Arguments.Return(args, true);
            }
        }

        private static object[] RentAndResize(int length)
        {
            var args = Arguments.Rent(length);
            Array.Resize(ref args, length);
            return args;
        }

        public abstract object Call(object target, object[] args);
    }
}