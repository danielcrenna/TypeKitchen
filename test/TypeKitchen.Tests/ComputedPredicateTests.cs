// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace TypeKitchen.Tests
{
	public class ComputedPredicateTests
	{
		[Fact]
		public void BasicTests_static_expression()
		{
			var computed = ComputedPredicate.Compute("1 == 1");
			Assert.True(computed);
		}

		[Fact]
		public void BasicTests_static_value()
		{
			var instance = new TestClass();
			var computed = ComputedPredicate.Compute(instance, "{{Value}} == \"Hello, World!\"");
			Assert.True(computed);
		}

		public class TestClass
		{
			public string Value => "Hello, World!";
		}
	}
}