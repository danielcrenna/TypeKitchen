// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CSharp.RuntimeBinder;

namespace TypeKitchen.Scripting
{
	public static class Snippet
	{
		private static readonly ScriptOptions DefaultOptions;
		private static readonly InteractiveAssemblyLoader Loader;
		
		static Snippet()
		{
			Loader = new InteractiveAssemblyLoader();

			var builder = new Scripting.ReferenceBuilder(ScriptOptions.Default);
			
			builder.Add<Guid>()					// System
					.Add<List<object>>()		// System.Collections.Generic
					.Add<Regex>()				// System.Text
					.Add<FileInfo>()			// System.IO
					.Add<IQueryable>()			// System.Linq;
					.Add<DynamicObject>()		// System.Dynamic
					.Add<CSharpArgumentInfo>()	// Microsoft.CSharp.RuntimeBinder
					.Add(typeof(Unsafe))		// System.Runtime.CompilerServices.Unsafe
				;

			DefaultOptions = builder.Build();
		}

		public static Scripting.ReferenceBuilder GetBuilder()
		{
			return new Scripting.ReferenceBuilder(DefaultOptions);
		}
		
		public static MethodInfo CreateMethod(string body, ScriptOptions options = null)
		{
			var type = CompileTypes(body, options)?[0];
			return type?.GetMethods()[1];
		}

		public static Type CreateType(string body, ScriptOptions options = null)
		{
			var type = CompileTypes(body, options)?[1];
			return type;
		}

		private static Type[] CompileTypes(string body, ScriptOptions options = null)
		{
			var script = CSharpScript.Create(body, options ?? DefaultOptions, typeof(ContextFree), Loader);
			var compilation = script.GetCompilation();

			using var pe = new MemoryStream();
			using var pdb = new MemoryStream();

			var result = compilation.Emit(pe, pdb);
			if (!result.Success)
			{
				foreach (var diagnostic in result.Diagnostics)
					Trace.TraceError($"Error or warning during snippet compilation: {diagnostic.GetMessage()}");
				return null;
			}

			pe.Seek(0, SeekOrigin.Begin);
			var assembly = AssemblyLoadContext.Default.LoadFromStream(pe, pdb);
			var types = assembly.GetTypes();
			return types;
		}

		private static class ContextFree { }
	}
}