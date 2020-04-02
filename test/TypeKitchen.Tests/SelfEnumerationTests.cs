// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace TypeKitchen.Tests
{
	public class SelfEnumerationTests
	{
		[Fact]
		public void Can_enumerate()
		{
			var expected = new List<string> {"A", "B", "C"};
			var actual = new List<string>();

			var enumerable = expected.Enumerate();
			foreach (var value in enumerable)
				actual.Add(value);

			actual.Clear();
			foreach (var value in enumerable)
				actual.Add(value);

			Assert.Equal(expected, actual);
			Assert.Equal(actual, enumerable.AsList);
		}
	}
}