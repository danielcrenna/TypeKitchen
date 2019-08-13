// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CSharp.RuntimeBinder;

namespace TypeKitchen
{
	public static class Snippet
	{
		private static readonly ScriptOptions DefaultOptions;
		private static readonly InteractiveAssemblyLoader Loader;

		private static ScriptOptions _options;

		static Snippet()
		{
			Loader = new InteractiveAssemblyLoader();

			DefaultOptions = ScriptOptions.Default
					.Add<Guid>() // System
					.Add<List<object>>() // System.Collections.Generic
					.Add<Regex>() // System.Text
					.Add<FileInfo>() // System.IO
					.Add<IQueryable>() // System.Linq;
					.Add<DynamicObject>() // System.Dynamic
					.Add<CSharpArgumentInfo>() // Microsoft.CSharp.RuntimeBinder
				;
		}

		public static void Add<T>()
		{
			_options = _options.Add<T>();
		}

		private static ScriptOptions Add<T>(this ScriptOptions root)
		{
			if (root == null)
				root = ScriptOptions.Default;
			var type = typeof(T);
			root = root.AddReferences(type.Assembly);
			root = root.AddImports(type.Namespace);
			return root;
		}

		public static MethodInfo CreateMethod(string body, ScriptOptions options = null)
		{
			var script = CSharpScript.Create(body, options ?? _options ?? DefaultOptions, typeof(ContextFree), Loader);
			var compilation = script.GetCompilation();

			using (var pe = new MemoryStream())
			using (var pdb = new MemoryStream())
			{
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
				var method = types[0].GetMethods()[1];
				return method;
			}
		}

		private static class ContextFree
		{
		}
	}
}