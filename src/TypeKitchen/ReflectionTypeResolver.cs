// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace TypeKitchen
{
	public class ReflectionTypeResolver : ITypeResolver
	{
		private readonly string[] _skipRuntimeAssemblies;
		private readonly Lazy<IEnumerable<MethodInfo>> _loadedMethods;
		private readonly Lazy<IEnumerable<Type>> _loadedTypes;

		public ReflectionTypeResolver(IEnumerable<Assembly> assemblies, IEnumerable<string> skipRuntimeAssemblies = null)
		{
			_loadedTypes = new Lazy<IEnumerable<Type>>(() => LoadTypes(assemblies, typeof(object).GetTypeInfo().Assembly));
			_loadedMethods = new Lazy<IEnumerable<MethodInfo>>(LoadMethods);
				
			_skipRuntimeAssemblies = new []
			{
				"Microsoft.VisualStudio.ArchitectureTools.PEReader",
				"Microsoft.IntelliTrace.Core"
			};
			if (skipRuntimeAssemblies != null)
				_skipRuntimeAssemblies = _skipRuntimeAssemblies.Concat(skipRuntimeAssemblies).ToArray();
		}

		public ReflectionTypeResolver() : this(AppDomain.CurrentDomain.GetAssemblies()) { }

		public Type FindByFullName(string typeName)
		{
			foreach (var type in _loadedTypes.Value)
				if (type.FullName != null && type.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
					return type;

			return null;
		}

		public Type FindFirstByName(string name)
		{
			foreach (var type in _loadedTypes.Value)
				if (type.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					return type;
			return null;
		}

		public Type FindFirstByMethodName(string methodName)
		{
			foreach (var method in _loadedMethods.Value)
				if (method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
					return method.DeclaringType;
			return null;
		}

		public IEnumerable<Type> FindByMethodName(string methodName)
		{
			var methods = _loadedMethods.Value;

			foreach (var m in methods)
				if (m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
					yield return m.DeclaringType;
		}

		public IEnumerable<Type> FindByInterface<TInterface>()
		{
			return FindByInterface(typeof(TInterface));
		}

		public IEnumerable<Type> FindByInterface(Type interfaceType)
		{
			foreach (var type in _loadedTypes.Value)
			{
				var info = type.GetTypeInfo();
				foreach (var @interface in info.ImplementedInterfaces)
					if (interfaceType == @interface)
						yield return type;
			}
		}

		public IEnumerable<Type> FindByParent<T>()
		{
			return FindByParent(typeof(T));
		}

		public IEnumerable<Type> FindByParent(Type parentType)
		{
			foreach (var type in _loadedTypes.Value)
			{
				if (type.IsSubclassOf(parentType))
					yield return type;
			}
		}

		private IEnumerable<MethodInfo> LoadMethods()
		{
			foreach (var type in _loadedTypes.Value)
			foreach (var method in type.GetMethods())
				yield return method;
		}

		private IEnumerable<Type> LoadTypes(IEnumerable<Assembly> assemblies, params Assembly[] skipAssemblies)
		{
			var types = new HashSet<Type>();

			foreach (var assembly in assemblies)
			{
				if (assembly.IsDynamic || ((IList) skipAssemblies).Contains(assembly) ||
				    ((IList) _skipRuntimeAssemblies).Contains(assembly.FullName))
					continue;

				try
				{
					foreach (var type in assembly.GetTypes()) types.Add(type);
				}
				catch (Exception e)
				{
					Trace.TraceError($"{e}");
				}
			}

			return types;
		}
	}
}