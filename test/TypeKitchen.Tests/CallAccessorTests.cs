// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
    public class CallAccessorTests
    {
        [Fact]
        public void Call_Type_Void_NoArgs()
        {
            var target = new ClassWithTwoMethodsAndProperty();
            var type = target.GetType();
            var accessor = CallAccessor.Create(type);
            Assert.Equal(type, accessor.Type);
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void Call_Method_Void_NoArgs()
        {
            var target = new ClassWithTwoMethodsAndProperty();
            var methodInfo = target.GetType().GetMethod("Foo");
            var accessor = CallAccessor.Create(methodInfo);
            Assert.Equal(methodInfo.Name, accessor.MethodName);

            var parameters = accessor.Parameters;
            Assert.NotNull(parameters);
            Assert.Equal(parameters, methodInfo.GetParameters());
            accessor.Call(target);
        }

        [Fact]
        public void Call_Method_Static_Void_NoArgs()
        {
            var target = new ClassWithTwoMethodsAndProperty();
            var methodInfo = target.GetType().GetMethod("Method");
            var accessor = CallAccessor.Create(methodInfo);
            Assert.Equal(methodInfo.Name, accessor.MethodName);
            var parameters = accessor.Parameters;
            Assert.NotNull(parameters);
            Assert.Equal(parameters, methodInfo.GetParameters());
            accessor.Call(target);
        }
    }
}