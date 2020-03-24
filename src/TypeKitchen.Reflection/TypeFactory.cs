// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WyHash;

namespace TypeKitchen.Reflection
{
	internal static class TypeFactory
	{
		private static readonly ConcurrentDictionary<ulong, Type> TypeCache;

		static TypeFactory()
		{
			TypeCache = new ConcurrentDictionary<ulong, Type>();
		}

		public static Type BuildAnonymousType(IDictionary<string, object> hash)
		{
			var cacheKey = GetCacheKey(hash);

			if (!TypeCache.TryGetValue(cacheKey, out var type))
				type = CreateAnonymousType(cacheKey, hash);

			return type;
		}

		private static Type CreateAnonymousType(ulong cacheKey, IDictionary<string, object> hash)
		{
			var tb = DynamicAssembly.Module.DefineType($"PseudoAnonymousType_{cacheKey}",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
				TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, null);

			tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName |
			                            MethodAttributes.RTSpecialName);

			foreach (var item in hash)
			{
				var propertyName = item.Key;
				var propertyType = item.Value?.GetType() ?? typeof(object);

				var backingField = tb.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
				var property = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

				var getMethod = tb.DefineMethod($"get_{propertyName}",
					MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType,
					Type.EmptyTypes);

				var getter = getMethod.GetILGeneratorInternal();
				getter.Ldarg_0();
				getter.Ldfld(backingField);
				getter.Ret();

				var setMethod = tb.DefineMethod($"set_{propertyName}",
					MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null,
					new[] {propertyType});

				var setter = setMethod.GetILGeneratorInternal();

				setter.Ldarg_0();
				setter.Ldarg_1();
				setter.Stfld(backingField);
				setter.Ret();

				property.SetGetMethod(getMethod);
				property.SetSetMethod(setMethod);
			}

			Type type = tb.CreateTypeInfo();
			TypeCache.TryAdd(cacheKey, type);
			return type;
		}

		private static ulong GetCacheKey(IDictionary<string, object> hash)
		{
			var sb = new StringBuilder();
			foreach (var item in hash.StableOrder(x => x.Key))
			{
				sb.Append(item.Key);
				sb.Append('_');
				sb.Append((item.Value?.GetType() ?? typeof(object)).FullName);
			}
			return WyHash64.ComputeHash64(sb.ToString(), 73173); /* 7yp3k17ch3n */
		}
	}
}