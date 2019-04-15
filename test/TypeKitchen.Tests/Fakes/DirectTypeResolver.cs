// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Tests.Fakes
{
    public sealed class DirectTypeResolver : ITypeResolver
    {
        private static readonly OnePropertyOneField Instance = new OnePropertyOneField();

        public T Resolve<T>()
        {
            if(typeof(T) == typeof(OnePropertyOneField))
                return (T)(object)Instance;
            return default;
        }

        private static OnePropertyOneField _instanceFromFunc;

        public T Singleton<T>(Func<ITypeResolver, T> builder)
        {
            if (typeof(T) == typeof(OnePropertyOneField))
                return (T)(object)(_instanceFromFunc ?? (_instanceFromFunc = (OnePropertyOneField)(object)builder(this)));
            return default;
        }
    }
}