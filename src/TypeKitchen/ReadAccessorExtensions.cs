// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using TypeKitchen.Internal;

namespace TypeKitchen
{
	public static class ReadAccessorExtensions
	{
		public static IReadOnlyDictionary<string, object> AsReadOnlyDictionary(this ITypeReadAccessor accessor,
			object instance)
		{
			return new ReadOnlyDictionaryWrapper(accessor, instance);
		}
	}
}