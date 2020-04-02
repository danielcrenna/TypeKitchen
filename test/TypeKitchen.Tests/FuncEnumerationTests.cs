// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace TypeKitchen.Tests
{
	public class FuncEnumerationTests
	{
		[Fact]
		public void Can_enumerate()
		{
			var expected = new List<Outer> {new Outer {Value = "A"}, new Outer {Value = "B"}, new Outer {Value = "C"}};
			var actual = new List<string>();

			var enumerable = expected.Enumerate(outer => outer.Value);
			foreach (var value in enumerable)
				actual.Add(value);

			actual.Clear();
			foreach (var value in enumerable)
				actual.Add(value);

			Assert.NotEmpty(actual);
			Assert.Equal(3, actual.Count);
		}

		public class Outer
		{
			public string Value { get; set; }
		}
	}
}