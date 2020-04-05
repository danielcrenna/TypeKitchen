// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
	public class WriteAccessorTests
	{
		[Fact]
		public void SetTests_PropertiesAndFields_Strings()
		{
			var target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};

			var get = ReadAccessor.Create(target.GetType());
			var set = WriteAccessor.Create(target.GetType());

			Assert.Equal("Bar", get[target, "Foo"]);
			Assert.Equal("Baz", get[target, "Bar"]);

			Assert.True(set.TrySetValue(target, "Foo", "Fizz"));
			Assert.True(set.TrySetValue(target, "Bar", "Buzz"));

			target = new OnePropertyOneFieldStrings {Foo = "Fizz", Bar = "Buzz"};
			var other = WriteAccessor.Create(target.GetType());
			Assert.Equal(set, other);
			set = other;

			Assert.Equal("Fizz", get[target, "Foo"]);
			Assert.Equal("Buzz", get[target, "Bar"]);

			set[target, "Foo"] = "Bar";
			set[target, "Bar"] = "Baz";

			Assert.Equal("Bar", get[target, "Foo"]);
			Assert.Equal("Baz", get[target, "Bar"]);

			Assert.Equal(typeof(OnePropertyOneFieldStrings), set.Type);
		}

		[Fact]
		public void SetTests_PropertiesAndFields_Integers()
		{
			var target = new OnePropertyOneFieldInts {Foo = 1, Bar = 2};

			var get = ReadAccessor.Create(target.GetType());
			var set = WriteAccessor.Create(target.GetType());

			Assert.Equal(1, get[target, "Foo"]);
			Assert.Equal(2, get[target, "Bar"]);

			Assert.True(set.TrySetValue(target, "Foo", 3));
			Assert.True(set.TrySetValue(target, "Bar", 4));

			target = new OnePropertyOneFieldInts { Foo = 3, Bar = 4 };
			var other = WriteAccessor.Create(target.GetType());
			Assert.Equal(set, other);
			set = other;

			Assert.Equal(3, get[target, "Foo"]);
			Assert.Equal(4, get[target, "Bar"]);

			set[target, "Foo"] = 5;
			set[target, "Bar"] = 6;

			Assert.Equal(5, get[target, "Foo"]);
			Assert.Equal(6, get[target, "Bar"]);

			Assert.Equal(typeof(OnePropertyOneFieldInts), set.Type);
		}

		[Fact]
		public void SetTests_can_write_value_type()
		{
			object target = new FooStruct { Foo = "Bar", Baz = 123 };
			var get = ReadAccessor.Create(target);
			
			var foo = get[target, nameof(FooStruct.Foo)];
			var baz = get[target, nameof(FooStruct.Baz)];

			Assert.Equal("Bar", foo);
			Assert.Equal(123, baz);

			var set = WriteAccessor.Create(target);

			Assert.True(set.TrySetValue(target, nameof(FooStruct.Foo), "Baz"));
			Assert.True(set.TrySetValue(target, nameof(FooStruct.Baz), 321));

			Assert.Equal("Baz", get[target, nameof(FooStruct.Foo)]);
			Assert.Equal(321, get[target, nameof(FooStruct.Baz)]);

			set[target, nameof(FooStruct.Foo)] = "Biff";
			set[target, nameof(FooStruct.Baz)] = 999;

			Assert.Equal("Biff", get[target, nameof(FooStruct.Foo)]);
			Assert.Equal(999, get[target, nameof(FooStruct.Baz)]);
		}

		[Fact]
		public void SetTests_can_write_non_public_members()
		{
			object target = new FooStructNonPublicMembers { Foo = "Bar", Baz = 123 };
			
			var get = ReadAccessor.Create(target);
			
			var foo = get[target, nameof(FooStructNonPublicMembers.Foo)];
			var baz = get[target, nameof(FooStructNonPublicMembers.Baz)];

			Assert.Equal("Bar", foo);
			Assert.Equal(123, baz);

			var set = WriteAccessor.Create(target);

			Assert.True(set.TrySetValue(target, nameof(FooStructNonPublicMembers.Foo), "Baz"));
			Assert.True(set.TrySetValue(target, nameof(FooStructNonPublicMembers.Baz), 321));

			Assert.Equal("Baz", get[target, nameof(FooStructNonPublicMembers.Foo)]);
			Assert.Equal(321, get[target, nameof(FooStructNonPublicMembers.Baz)]);

			set[target, nameof(FooStructNonPublicMembers.Foo)] = "Biff";
			set[target, nameof(FooStructNonPublicMembers.Baz)] = 999;

			Assert.Equal("Biff", get[target, nameof(FooStructNonPublicMembers.Foo)]);
			Assert.Equal(999, get[target, nameof(FooStructNonPublicMembers.Baz)]);
		}

		public struct FooStruct
		{
			public string Foo { get; set; }
			public int Baz;
		}

		public struct FooStructNonPublicMembers
		{
			internal string Foo { get; set; }
			internal int Baz;
		}
	}
}