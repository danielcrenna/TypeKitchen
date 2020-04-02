// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace TypeKitchen.Tests
{
	public class PredicateEnumerationTests
	{
		[Fact]
		public void Can_enumerate()
		{
			var expected = new List<Outer> { new Outer { Value = "A" }, new Outer { Value = "B" }, new Outer { Value = "A" } };
			var actual = new List<string>();

			var enumerable = expected.Enumerate(new Predicate<Outer>(outer => outer.Value == "A"));
			foreach (var item in enumerable)
				actual.Add(item.Value);

			actual.Clear();
			foreach (var item in enumerable)
				actual.Add(item.Value);

			Assert.NotEmpty(actual);
			Assert.Equal(2, actual.Count);
			Assert.All(actual, s => Assert.Equal("A", s));
		}

		public class Outer
		{
			public string Value { get; set; }
		}
	}
}