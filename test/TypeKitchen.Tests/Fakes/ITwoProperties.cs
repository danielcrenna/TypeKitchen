// Copyright (c) Blowdart, Inc. All rights reserved.
namespace TypeKitchen.Tests.Fakes
{
    public interface ITwoProperties
    {
        string Foo { get; }
        string Bar { get; }
    }


    public interface ITwoMethodsAndProperty
    {
        void Foo();
        void Bar(int i);
        string Baz { get; }
    }
}