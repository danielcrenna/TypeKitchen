// Copyright (c) Blowdart, Inc. All rights reserved.

using System;

namespace TypeKitchen.Tests.Fakes
{
    public sealed class DirectWriteAccessorForOnePropertyOneField : ITypeWriteAccessor
    {
        public static DirectWriteAccessorForOnePropertyOneField Instance =
            new DirectWriteAccessorForOnePropertyOneField();

        private DirectWriteAccessorForOnePropertyOneField()
        {
        }

        public bool TrySetValue(object target, string key, object value)
        {
            switch (key)
            {
                case "Foo":
                    ((OnePropertyOneField) target).Foo = (string) value;
                    return true;
                case "Bar":
                    ((OnePropertyOneField) target).Bar = (string) value;
                    return true;
                default:
                    return false;
            }
        }

        public object this[object target, string key]
        {
            set
            {
                switch (key)
                {
                    case "Foo":
                        ((OnePropertyOneField) target).Foo = (string) value;
                        return;
                    case "Bar":
                        ((OnePropertyOneField) target).Bar = (string) value;
                        return;
                    default:
                        throw new ArgumentNullException();
                }
            }
        }
    }
}