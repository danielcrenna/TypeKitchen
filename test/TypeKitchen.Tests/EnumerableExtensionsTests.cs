// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TypeKitchen.Tests
{
	public class EnumerableExtensionsTests
	{
		[Fact]
		public void Enumerable_is_already_a_list()
		{
			IEnumerable<string> enumerable = new List<string>();
			var list = enumerable.AsList();
			Assert.Equal(enumerable, list);
			Assert.StrictEqual(enumerable, list);
		}

		[Fact]
		public void Enumerable_is_not_a_list()
		{
			var enumerable = Enumerable.Repeat(1, 10);

			// ReSharper disable once PossibleMultipleEnumeration
			var list = enumerable.AsList();

			// ReSharper disable once PossibleMultipleEnumeration
			Assert.Equal(enumerable, list);

			// ReSharper disable once PossibleMultipleEnumeration
			Assert.NotStrictEqual(enumerable, list);
		}
	}
}