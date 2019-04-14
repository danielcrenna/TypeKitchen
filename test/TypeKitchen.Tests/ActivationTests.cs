// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
    public class ActivationTests
    {
        [Fact]
        public void DynamicMethod_NoParams()
        {
            var target = Activation.DynamicMethodWeakTyped(typeof(ClassWithTwoMethodsAndProperty).GetConstructor(Type.EmptyTypes))();
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void DynamicMethod_Params()
        {
            var target = Activation.DynamicMethodWeakTyped(typeof(ClassWithTwoMethodsAndProperty).GetConstructor(new[] { typeof(int)} ))(100);
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void Expression_NoParams()
        {
            var target = Activation.ExpressionWeakTyped(typeof(ClassWithTwoMethodsAndProperty).GetConstructor(Type.EmptyTypes))();
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void Expression_Params()
        {
            var target = Activation.DynamicMethodWeakTyped(typeof(ClassWithTwoMethodsAndProperty).GetConstructor(new[] { typeof(int) }))(100);
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void Activator_NoParams()
        {
            var target = Activation.ActivatorWeakTyped(typeof(ClassWithTwoMethodsAndProperty).GetConstructor(Type.EmptyTypes))();
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void Activator_Params()
        {
            var target = Activation.ActivatorWeakTyped(typeof(ClassWithTwoMethodsAndProperty).GetConstructor(new[] { typeof(int) }))(100);
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void Invoke_NoParams()
        {
            var target = Activation.InvokeWeakTyped(typeof(ClassWithTwoMethodsAndProperty).GetConstructor(Type.EmptyTypes))();
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void Invoke_Params()
        {
            var target = Activation.InvokeWeakTyped(typeof(ClassWithTwoMethodsAndProperty).GetConstructor(new[] { typeof(int) }))(100);
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }
    }
}