// Copyright (c) Blowdart, Inc. All rights reserved.

namespace TypeKitchen.Tests.Fakes
{
    public sealed class DirectDuckCastProxy : ITwoMethodsAndProperty
    {
        private readonly ITwoMethodsAndProperty _instance;

        public DirectDuckCastProxy(ITwoMethodsAndProperty instance)
        {
            _instance = instance;
        }

        public void Foo() => _instance.Foo();
        public void Bar(int i) => _instance.Bar(i);
        public string Baz => _instance.Baz;
    }
}