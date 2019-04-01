// Copyright (c) Blowdart, Inc. All rights reserved.

using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
    public class ReadAccessorTests
    {
        [Fact]
        public void GetTests_AnonymousType()
        {
            var target = GetOutOfMethodTarget();
            var hash = ReadAccessor.Create(target.GetType());
            var foo = hash[target, "Foo"];
            var bar = hash[target, "Bar"];
            Assert.Equal("Bar", foo);
            Assert.Equal("Baz", bar);

            Assert.True(hash.TryGetValue(target, "Bar", out var value));
            Assert.Equal("Baz", value);

            target = new {Foo = "Fizz", Bar = "Buzz"};
            var other = ReadAccessor.Create(target.GetType());
            Assert.Equal(hash, other);
            hash = other;

            foo = hash[target, "Foo"];
            bar = hash[target, "Bar"];
            Assert.Equal("Fizz", foo);
            Assert.Equal("Buzz", bar);

            Assert.True(hash.TryGetValue(target, "Bar", out value));
            Assert.Equal("Buzz", value);
        }

        [Fact]
        public void GetTests_PropertiesAndFields()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var hash = ReadAccessor.Create(target.GetType());
            var foo = hash[target, "Foo"];
            var bar = hash[target, "Bar"];
            Assert.Equal("Bar", foo);
            Assert.Equal("Baz", bar);

            Assert.True(hash.TryGetValue(target, "Bar", out var value));
            Assert.Equal("Baz", value);

            target = new OnePropertyOneField {Foo = "Fizz", Bar = "Buzz"};
            var other = ReadAccessor.Create(target.GetType());
            Assert.Equal(hash, other);
            hash = other;

            foo = hash[target, "Foo"];
            bar = hash[target, "Bar"];
            Assert.Equal("Fizz", foo);
            Assert.Equal("Buzz", bar);

            Assert.True(hash.TryGetValue(target, "Bar", out value));
            Assert.Equal("Buzz", value);
        }

        public object GetOutOfMethodTarget()
        {
            return new { Foo = "Bar", Bar = "Baz" };
        }
    }
}