// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;
using TypeKitchen.Composition;
using TypeKitchen.Composition.Internal;

namespace TypeKitchen.Tests.Composition
{
	public class CombinationTests
	{
		[Fact]
		public void Can_get_k_combinations_of_n()
		{
			var combinations = new[] { "A", "B", "C"}.GetCombinations(1);
			Assert.NotNull(combinations);
			Assert.Equal(3, combinations.Distinct().Count());
		}
	}
}

