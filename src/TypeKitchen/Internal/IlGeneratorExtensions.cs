// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection.Emit;

namespace TypeKitchen.Internal
{
    internal static class IlGeneratorExtensions
    {
        public static ILSugar GetILGeneratorInternal(this MethodBuilder b)
        {
            return new ILSugar(b.GetILGenerator());
        }
    }
}