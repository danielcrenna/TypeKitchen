// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace TypeKitchen
{
	public struct PredicateEnumerable<T>
	{
		private readonly Predicate<T> _predicate;

		public PredicateEnumerable(List<T> inner, Predicate<T> predicate)
		{
			AsList = inner;
			_predicate = predicate;
		}

		public PredicateEnumerator<T> GetEnumerator()
		{
			return new PredicateEnumerator<T>(AsList, _predicate);
		}

		public List<T> AsList { get; }
	}
}