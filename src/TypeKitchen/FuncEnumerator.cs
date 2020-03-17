// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace TypeKitchen
{
	public struct FuncEnumerator<T, TResult>
	{
		private readonly List<T> _inner;
		private readonly Func<T, TResult> _func;
		private int _index;

		public FuncEnumerator(List<T> inner, Func<T, TResult> func)
		{
			_inner = inner;
			_func = func;
			_index = 0;
		}

		public TResult Current => _inner == null || _index == 0 ? default : _func(_inner[_index - 1]);

		public bool MoveNext()
		{
			_index++;
			return _inner != null && _inner.Count >= _index;
		}
	}
}