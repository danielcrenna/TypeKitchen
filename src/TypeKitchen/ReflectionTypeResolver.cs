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

		public Type FindByFullName(string typeName) => _loadedTypes.Value.SingleOrDefault(t => t.FullName != null && t.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase));

		public Type FindFirstByName(string name) => _loadedTypes.Value.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

		public Type FindFirstByMethodName(string methodName) => _loadedMethods.Value.FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))?.DeclaringType;

		public IEnumerable<Type> FindByMethodName(string methodName) => _loadedMethods.Value.Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)).Select(x => x.DeclaringType);
	}
}
