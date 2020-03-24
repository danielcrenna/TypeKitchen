// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeKitchen.Reflection
{
	internal static class EnumerableExtensions
	{
		public static IEnumerable<T> StableOrder<T>(this IEnumerable<T> enumerable, Func<T, string> getName)
		{
			return enumerable.OrderBy(getName, StringComparer.Ordinal);
		}
	}
}