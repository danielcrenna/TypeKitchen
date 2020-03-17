// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace TypeKitchen
{
	public struct FuncEnumerable<T, TResult>
	{
		private readonly Func<T, TResult> _func;

		public FuncEnumerable(List<T> inner, Func<T, TResult> func)
		{
			AsList = inner;
			_func = func;
		}

		public FuncEnumerator<T, TResult> GetEnumerator()
		{
			return new FuncEnumerator<T, TResult>(AsList, _func);
		}

		public List<T> AsList { get; }
	}
}