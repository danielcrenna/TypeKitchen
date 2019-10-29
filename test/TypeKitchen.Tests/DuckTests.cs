// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace TypeKitchen.Tests
{
	public class DuckTests
	{
		[Fact]
		public void Can_duck_cast_class_to_interface()
		{
			IFoo foo = new Foo { Bar = "Baz" }.QuackLike<IFoo>();
			Assert.Equal("Baz", foo.Bar);
		}

		[Fact]
		public void Can_duck_cast_struct_to_interface()
		{
			IBar foo = new BazStruct { Bar = 123 }.QuackLike<IBar>();
			Assert.Equal(123, foo.Bar);
		}

		[Fact]
		public void Can_duck_cast_struct_to_struct()
		{
			BazStruct foo = new FooStruct { Bar = 123 }.QuackLike<BazStruct>();
			Assert.Equal(123, foo.Bar);
		}

		[Fact]
		public void Can_duck_cast_class_to_class()
		{
			Biff biff = new Foo { Bar = "123" }.QuackLike<Biff>();
			Assert.Equal("123", biff.Bar);
		}

		[Fact]
		public void Can_duck_cast_class_to_struct()
		{
			BazStruct foo = new Baz { Bar = 123 }.QuackLike<BazStruct>();
			Assert.Equal(123, foo.Bar);
		}

		[Fact]
		public void Can_duck_cast_struct_to_class()
		{
			Baz foo = new BazStruct { Bar = 123 }.QuackLike<Baz>();
			Assert.Equal(123, foo.Bar);
		}

		public class Foo
		{
			public string Bar { get; set; }
		}

		public class Biff
		{
			public string Bar { get; set; }
		}

		public class Baz
		{
			public int Bar;
		}

		public struct FooStruct
		{
			public int Bar { get; set; }
		}

		public struct BazStruct
		{
			public int Bar;
		}

		public interface IFoo
		{
			string Bar { get; }
		}

		public interface IBar
		{
			int Bar { get; }
		}
	}
}
