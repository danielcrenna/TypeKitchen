using System;

namespace ExternalTestAssembly
{
    public static class AnonymousTypeFactory
    {
        public static object Foo()
        {
            return new {Foo = "Foo"};
        }

        public static object Bar()
        {
            return new {Bar = "Bar"};
        }
    }
}
