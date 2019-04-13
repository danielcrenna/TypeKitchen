// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Tests.Fakes
{
    public sealed class DirectWriteAccessor : ITypeWriteAccessor
    {
        public static DirectWriteAccessor Instance = new DirectWriteAccessor();

        private DirectWriteAccessor()
        {
        }

        public Type Type => typeof(OnePropertyOneField);

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