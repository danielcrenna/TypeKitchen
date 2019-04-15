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
            var accessor = CallAccessor.Create(target.GetType());
            accessor.Call(target, "Foo");
        }

        [Fact]
        public void Call_Method_Void_NoArgs()
        {
            var target = new ClassWithTwoMethodsAndProperty();
            var accessor = CallAccessor.Create(target.GetType().GetMethod("Foo"));
            accessor.Call(target);
        }

        [Fact]
        public void Call_Static_Method_Void_NoArgs()
        {
            var target = new ClassWithTwoMethodsAndProperty();
            var accessor = CallAccessor.Create(target.GetType().GetMethod("Method"));
            accessor.Call(target);
        }
    }
}