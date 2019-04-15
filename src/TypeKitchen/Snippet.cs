// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CSharp.RuntimeBinder;

namespace TypeKitchen
{
    public sealed class Snippet
    {
        private static readonly ScriptOptions DefaultOptions;
        private static readonly InteractiveAssemblyLoader Loader;

        private static ScriptOptions _options;

        static Snippet()
        {
            Loader = new InteractiveAssemblyLoader();

            DefaultOptions = ScriptOptions.Default
                .AddReferences(Assembly.GetAssembly(typeof(DynamicObject)), Assembly.GetAssembly(typeof(CSharpArgumentInfo)), Assembly.GetAssembly(typeof(File)))
                .AddImports(typeof(DynamicObject).Namespace, typeof(CSharpArgumentInfo).Namespace, typeof(File).Namespace);
        }

        public static void Add<T>()
        {
            if (_options == null)
                _options = ScriptOptions.Default;

            var type = typeof(T);

            _options = _options.AddReferences(type.Assembly);
            _options = _options.AddImports(type.Namespace);
        }

        private static class ContextFree { }

        public static MethodInfo CreateMethod(string body)
        {
            var script = CSharpScript.Create(body, _options ?? DefaultOptions, typeof(ContextFree), Loader);
            var compilation = script.GetCompilation();
            
            using (var pe = new MemoryStream())
            {
                using (var pdb = new MemoryStream())
                {
                    var result = compilation.Emit(pe, pdb);

                    pe.Seek(0, SeekOrigin.Begin);

                    var assembly = AssemblyLoadContext.Default.LoadFromStream(pe, pdb);
                    var types = assembly.GetTypes();
                    var method = types[0].GetMethods()[1];
                    return method;
                }
            }
        }
    }
}