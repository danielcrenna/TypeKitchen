// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace TypeKitchen
{
	public class ReflectionTypeResolver : ITypeResolver
	{
		private readonly Lazy<IEnumerable<Type>> _loadedTypes;
		private readonly Lazy<IEnumerable<MethodInfo>> _loadedMethods;

		public ReflectionTypeResolver(IEnumerable<Assembly> assemblies)
		{
			_loadedTypes = new Lazy<IEnumerable<Type>>(() => LoadTypes(assemblies, typeof(object).GetTypeInfo().Assembly));
			_loadedMethods = new Lazy<IEnumerable<MethodInfo>>(LoadMethods);
		}

		private IEnumerable<MethodInfo> LoadMethods()
		{
			foreach (var type in _loadedTypes.Value)
			{
				foreach (var method in type.GetMethods())
				{
					yield return method;
				}
			}
		}

		private static string[] SkipRuntimeAssemblies = { "Microsoft.VisualStudio.ArchitectureTools.PEReader"};

		private static IEnumerable<Type> LoadTypes(IEnumerable<Assembly> assemblies, params Assembly[] skipAssemblies)
		{
			var types = new HashSet<Type>();

			foreach(var assembly in assemblies)
			{
				if (assembly.IsDynamic || ((IList) skipAssemblies).Contains(assembly) || ((IList) SkipRuntimeAssemblies).Contains(assembly.FullName))
					continue;

				try
				{
					foreach (var type in assembly.GetTypes())
					{
						types.Add(type);
					}
				}
				catch (Exception e)
				{
					Trace.TraceError($"{e}");
				}
			}

			return types;
		}

		public ReflectionTypeResolver() : this(AppDomain.CurrentDomain.GetAssemblies()) { }

		public Type FindByFullName(string typeName)
		{
			foreach (var type in _loadedTypes.Value)
			{
				if (type.FullName != null && type.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
					return type;
			}

			return null;
		}

		public Type FindFirstByName(string name)
		{
			foreach (var type in _loadedTypes.Value)
			{
				if (type.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					return type;
			}
			return null;
		}

		public Type FindFirstByMethodName(string methodName)
		{
			foreach (var method in _loadedMethods.Value)
			{
				if (method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
					return method.DeclaringType;
			}
			return null;
		}

		public IEnumerable<Type> FindByMethodName(string methodName)
		{
			var methods = _loadedMethods.Value;

			foreach (var m in methods)
			{
				if (m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
					yield return m.DeclaringType;
			}
		}
	}
}
