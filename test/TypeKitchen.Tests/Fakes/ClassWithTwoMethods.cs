// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen.Tests.Fakes
{
    public class ClassWithTwoMethodsAndProperty : ITwoMethodsAndProperty
    {
        public int Count;

        public void Foo()
        {
            Count++;
        }

        public void Bar(int i)
        {
            Count += i;
        }

        public string Baz => "ABC";
    }
}