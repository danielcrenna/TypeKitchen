// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Reflection.Emit;

namespace TypeKitchen.Internal
{
    internal static class DynamicAssembly
    {
        public static readonly AssemblyName Name;
        public static readonly AssemblyBuilder Builder;
        public static readonly ModuleBuilder Module;

        static DynamicAssembly()
        {
            Name = new AssemblyName("__TypeKitchen");
            Builder = AssemblyBuilder.DefineDynamicAssembly(Name, AssemblyBuilderAccess.Run);
            Module = Builder.DefineDynamicModule(Name.Name);
        }
    }
}