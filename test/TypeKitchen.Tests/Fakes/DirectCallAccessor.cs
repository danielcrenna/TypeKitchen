// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace TypeKitchen.Tests.Fakes
{
    public sealed class DirectCallAccessor : ITypeCallAccessor, IMethodCallAccessor
    {
        public Type Type => typeof(ClassWithTwoMethodsAndProperty);

        public object Call(object target, string key, params object[] args)
        {
            switch (key)
            {
                case "Foo":
                    ((ClassWithTwoMethodsAndProperty)target).Foo();
                    return typeof(void);
                case "Bar":
                    ((ClassWithTwoMethodsAndProperty)target).Bar((int) args[0]);
                    return typeof(void);
                case "Method":
                    return typeof(ClassWithTwoMethodsAndProperty).GetMethod("Method").Invoke(null, null);
                default:
                    throw new ArgumentNullException();
            }
        }

        public MethodInfo MethodInfo => typeof(ClassWithTwoMethodsAndProperty).GetMethod(nameof(ClassWithTwoMethodsAndProperty.Foo));
        public object Call(object target, params object[] args)
        {
            ((ClassWithTwoMethodsAndProperty)target).Foo();
            return null;
        }
    }
}