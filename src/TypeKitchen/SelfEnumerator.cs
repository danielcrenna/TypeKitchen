// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace TypeKitchen
{
	public struct SelfEnumerator<T>
	{
		private readonly List<T> _inner;
		private int _index;

		public SelfEnumerator(List<T> inner)
		{
			_inner = inner;
			_index = 0;
		}

		public T Current => _inner == null || _index == 0 ? default : _inner[_index - 1];

		public bool MoveNext()
		{
			_index++;
			return _inner != null && _inner.Count >= _index;
		}
	}
}