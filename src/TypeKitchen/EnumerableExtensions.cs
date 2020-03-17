// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeKitchen
{
	public static class EnumerableExtensions
	{
		public static SelfEnumerable<T> Enumerate<T>(this List<T> inner)
		{
			return new SelfEnumerable<T>(inner);
		}

		public static FuncEnumerable<T, TResult> Enumerate<T, TResult>(this List<T> inner, Func<T, TResult> func)
		{
			return new FuncEnumerable<T, TResult>(inner, func);
		}

		public static PredicateEnumerable<T> Enumerate<T>(this List<T> inner, Predicate<T> predicate)
		{
			return new PredicateEnumerable<T>(inner, predicate);
		}

		public static IList<T> AsList<T>(this IEnumerable<T> enumerable)
		{
			return enumerable as IList<T> ?? enumerable.ToList();
		}
	}
}