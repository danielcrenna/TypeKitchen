// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeKitchen.Internal
{
	internal static class EnumerableExtensions
	{
		public static IEnumerable<T> NetworkOrder<T>(this IEnumerable<T> enumerator, Func<T, string> getName)
		{
			return enumerator.OrderBy(getName, StringComparer.Ordinal);
		}
	}
}