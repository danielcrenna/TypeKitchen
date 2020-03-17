// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace TypeKitchen.Internal
{
	public static class NamingExtensions
	{
		public static string CreateNameForProxy(this Type type, ProxyType proxyType)
		{
			return $"{proxyType}_Proxy_{type.Name}_{TypeHash(type, nameof(CreateNameForProxy))}";
		}

		public static string CreateNameForMethodCallAccessor(this Type type, MethodInfo method)
		{
			return $"Call_{method.Name}_{TypeHash(type, nameof(CreateNameForMethodCallAccessor))}";
		}

		public static string CreateNameForCallAccessor(this Type type)
		{
			return $"Call_{type.Name}_{TypeHash(type, nameof(CreateNameForCallAccessor))}";
		}

		public static string CreateNameForReadAccessor(this Type type, AccessorMemberTypes types,
			AccessorMemberScope scope)
		{
			return $"Read_{type.Name}_{types}_{scope}_{TypeHash(type, nameof(CreateNameForReadAccessor))}";
		}

		public static string CreateNameForWriteAccessor(this Type type, AccessorMemberTypes types,
			AccessorMemberScope scope)
		{
			return $"Write_{type.Name}_{types}_{scope}_{TypeHash(type, nameof(CreateNameForWriteAccessor))}";
		}

		private static Value128 TypeHash(Type type, string entropy = "", Value128 seed = default)
		{
			var hash = Hashing.MurmurHash3(type.AssemblyQualifiedName) ^
			           Hashing.MurmurHash3(entropy) ^
			           seed;

			return hash;
		}
	}
}