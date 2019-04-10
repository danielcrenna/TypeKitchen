// Copyright (c) Blowdart, Inc. All rights reserved.

using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
    public class WriteAccessorTests
    {
        [Fact]
        public void SetTests_PropertiesAndFields()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};

            var get = ReadAccessor.Create(target.GetType());
            var set = WriteAccessor.Create(target.GetType());

            Assert.Equal("Bar", get[target, "Foo"]);
            Assert.Equal("Baz", get[target, "Bar"]);

            Assert.True(set.TrySetValue(target, "Foo", "Fizz"));
            Assert.True(set.TrySetValue(target, "Bar", "Buzz"));

            target = new OnePropertyOneField {Foo = "Fizz", Bar = "Buzz"};
            var other = WriteAccessor.Create(target.GetType());
            Assert.Equal(set, other);
            set = other;

            Assert.Equal("Fizz", get[target, "Foo"]);
            Assert.Equal("Buzz", get[target, "Bar"]);

            set[target, "Foo"] = "Bar";
            set[target, "Bar"] = "Baz";

            Assert.Equal("Bar", get[target, "Foo"]);
            Assert.Equal("Baz", get[target, "Bar"]);

            Assert.Equal(typeof(OnePropertyOneField), set.Type);
        }
    }
}