// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace TypeKitchen
{
	public struct PredicateEnumerator<T>
	{
		private readonly List<T> _inner;
		private readonly Predicate<T> _predicate;
		private int _index;

		public PredicateEnumerator(List<T> inner, Predicate<T> predicate)
		{
			_inner = inner;
			_predicate = predicate;
			_index = 0;
		}

		public T Current => GetCurrentValue();

		private T GetCurrentValue()
		{
			return _inner == null || _index == 0 ? default :
				_predicate(_inner[_index - 1]) ? _inner[_index - 1] : default;
		}

		public bool MoveNext()
		{
			_index++;
			var more = _inner != null && _inner.Count >= _index;
			while (more && !_predicate(_inner[_index - 1]))
				MoveNext();
			return more;
		}
	}
}