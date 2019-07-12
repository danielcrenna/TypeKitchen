// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeKitchen
{
	public class ReflectionTypeResolver : ITypeResolver
	{
		private readonly Lazy<IEnumerable<Type>> _loadedTypes;
		private readonly Lazy<IEnumerable<MethodInfo>> _loadedMethods;

		public ReflectionTypeResolver(IEnumerable<Assembly> assemblies)
		{
			_loadedTypes = new Lazy<IEnumerable<Type>>(() =>
			{
				var mscorlib = typeof(object).GetTypeInfo().Assembly;
				return assemblies.Where(a => !a.IsDynamic && a != mscorlib).SelectMany(a => a.GetTypes());
			});
			_loadedMethods = new Lazy<IEnumerable<MethodInfo>>(() =>
			{
				return _loadedTypes.Value.SelectMany(x => x.GetMethods());
			});
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
