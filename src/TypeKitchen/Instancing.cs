// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace TypeKitchen
{
    public static class Instancing
    {
        public static readonly Dictionary<Type, CreateInstance> Factory = new Dictionary<Type, CreateInstance>();

        private static readonly ArrayPool<object> ArgumentsPool = ArrayPool<object>.Create();

        public static T CreateInstance<T>() => (T)GetOrBuildActivator<T>()();

        public static object CreateInstance(Type type)
        {
            var activator = GetOrBuildActivator(type);

            return activator();
        }

        public static object CreateInstance(Type type, params object[] args)
        {
            var activator = GetOrBuildActivator(type);

            return activator(args);
        }

        public static object CreateInstance(Type type, IServiceProvider serviceProvider)
        {
            var activator = GetOrBuildActivator(type);

            var parameters = activator.Method.GetParameters();
            var args = ArgumentsPool.Rent(parameters.Length);

            try
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    args[i] = serviceProvider.GetService(parameter.ParameterType);
                }
                var instance = activator(args);
                return instance;
            }
            finally
            {
                ArgumentsPool.Return(args, true);
            }
        }

        private static CreateInstance GetOrBuildActivator<T>() => GetOrBuildActivator(typeof(T));
        private static CreateInstance GetOrBuildActivator(Type type)
        {
            lock (Factory)
            {
                if (Factory.TryGetValue(type, out var activator))
                    return activator;

                lock (Factory)
                {
                    if (Factory.TryGetValue(type, out activator))
                        return activator;

                    var ctor = type.GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();
                    Factory.Add(type, activator = Activation.DynamicMethodWeakTyped(ctor));
                }
                return activator;
            }
        }
    }
}