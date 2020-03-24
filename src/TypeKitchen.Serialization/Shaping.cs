// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using TypeKitchen.Creation;
using TypeKitchen.Reflection;

namespace TypeKitchen.Serialization
{
	public static class Shaping
	{
		public static object ToAnonymousObject(IDictionary<string, object> hash)
		{
			var anonymousType = TypeFactory.BuildAnonymousType(hash);
			var instance = Instancing.CreateInstance(anonymousType);
			var accessor = WriteAccessor.Create(anonymousType, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var members);

			foreach (var member in members)
			{
				if (!member.CanWrite)
					continue; // should be accessible by design

				if (!hash.TryGetValue(member.Name, out var value))
					continue; // should be mapped one-to-one

				accessor.TrySetValue(instance, member.Name, value);
			}

			return instance;

		}
	}
}