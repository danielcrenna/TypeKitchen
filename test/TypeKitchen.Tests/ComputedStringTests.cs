// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace TypeKitchen.Tests
{
	public class ComputedStringTests
	{
		[Fact]
		public void BasicTests_static_expression()
		{
			var computed = ComputedString.Compute("Hello, World!");
			Assert.Equal("Hello, World!", computed);
		}

		[Fact]
		public void BasicTests_static_value()
		{
			var instance = new TestClass();
			var computed = ComputedString.Compute(instance, "{{Value}}");
			Assert.Equal("Hello, World!", computed);
		}

		[Fact]
		public void BasicTests_nested_value()
		{
			var instance = new TestClass();
			var computed = ComputedString.Compute(instance, "{{Nested.Value}}");
			Assert.Equal("Hello, World!", computed);
		}

		[Fact]
		public void BasicTests_indirect_nested_value()
		{
			var instance = new TestClass();
			var computed = ComputedString.Compute(instance, "{{Indirect.Value}}");
			Assert.Equal("Hello, World!", computed);
		}

		[Fact]
		public void BasicTests_invocation_with_no_parameters()
		{
			var instance = new TestClass();
			var computed = ComputedString.Compute(instance, "{{Invoke.GetValue()}}");
			Assert.Equal("Hello, World!", computed);
		}
		
		#region Fakes 

		public class TestClass
		{
			public string Value => "Hello, World!";
			public Invoke Invoke { get; set; } = new Invoke();
			public Indirect Indirect { get; set; } = new Indirect();
			public Nested Nested { get; set; } = new Nested();
		}

		public class Invoke
		{
			public string GetValue()
			{
				return "Hello, World!";
			}

			public string GetValueWithParameter(string world)
			{
				return $"Hello, {world}!";
			}
		}

		public class Indirect
		{
			public string Value => ComputedString.Compute("Hello, World!");
		}

		public class Nested
		{
			public string Value => "Hello, World!";
		}

		#endregion
	}
}