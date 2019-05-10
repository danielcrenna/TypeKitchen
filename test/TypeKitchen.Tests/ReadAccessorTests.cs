// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using ExternalTestAssembly;
using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
    public class ReadAccessorTests
    {
        [Fact]
        public void GetTests_Caching_External()
        {
            var target = AnonymousTypeFactory.Foo();
            var accessor1 = ReadAccessor.Create(target.GetType());

            var target2 = AnonymousTypeFactory.Bar();
            var accessor2 = ReadAccessor.Create(target2.GetType());

            Assert.NotEqual(accessor1, accessor2);

            var target3 = GetOutOfMethodTarget();
            var accessor3 = ReadAccessor.Create(target3.GetType());

            Assert.NotEqual(accessor3, accessor1);
            Assert.NotEqual(accessor3, accessor2);
        }

        [Fact]
        public void GetTests_AnonymousType()
        {
            var target = GetOutOfMethodTarget();
            var accessor = ReadAccessor.Create(target.GetType());
            var foo = accessor[target, "Foo"];
            var bar = accessor[target, "Bar"];
            Assert.Equal("Bar", foo);
            Assert.Equal("Baz", bar);

            Assert.True(accessor.TryGetValue(target, "Bar", out var value));
            Assert.Equal("Baz", value);

            target = new {Foo = "Fizz", Bar = "Buzz"};
            var other = ReadAccessor.Create(target.GetType());
            Assert.Equal(accessor, other);
            accessor = other;

            foo = accessor[target, "Foo"];
            bar = accessor[target, "Bar"];
            Assert.Equal("Fizz", foo);
            Assert.Equal("Buzz", bar);

            Assert.True(accessor.TryGetValue(target, "Bar", out value));
            Assert.Equal("Buzz", value);
        }

        [Fact]
        public void GetTests_PropertiesAndFields()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var accessor = ReadAccessor.Create(target.GetType());
            var foo = accessor[target, "Foo"];
            var bar = accessor[target, "Bar"];
            Assert.Equal("Bar", foo);
            Assert.Equal("Baz", bar);

            Assert.True(accessor.TryGetValue(target, "Bar", out var value));
            Assert.Equal("Baz", value);

            target = new OnePropertyOneField {Foo = "Fizz", Bar = "Buzz"};
            var other = ReadAccessor.Create(target.GetType());
            Assert.Equal(accessor, other);
            accessor = other;

            foo = accessor[target, "Foo"];
            bar = accessor[target, "Bar"];
            Assert.Equal("Fizz", foo);
            Assert.Equal("Buzz", bar);

            Assert.True(accessor.TryGetValue(target, "Bar", out value));
            Assert.Equal("Buzz", value);

            Assert.Equal(typeof(OnePropertyOneField), accessor.Type);
        }

        public object GetOutOfMethodTarget()
        {
            return new { Foo = "Bar", Bar = "Baz" };
        }

        [Fact]
        public void GetTests_DictionaryWrapper()
        {
            var target = new OnePropertyOneField { Foo = "Bar", Bar = "Baz" };
            var accessor = ReadAccessor.Create(target.GetType());
            var dict = accessor.AsReadOnlyDictionary(target);
            Assert.Equal("Bar", dict["Foo"]);
            Assert.Equal("Baz", dict["Bar"]);
        }


        [Fact]
        public void GetTests_ComplexNestedType_ShortFormCheck()
        {
            // this will fail if Brtrue_S is ever incorrectly chosen over Brtrue.
            ReadAccessor.Create(typeof(ComplexNestedType));
        }
        

        #region Complex Nest Type

        public class FeatureToggle
        {
            public bool Enabled { get; set; } = true;
        }

        public class ComplexNestedType
        {
            public string AssemblyName { get; set; } = Assembly.GetExecutingAssembly().GetName()?.Name;
            public string AssemblyVersion { get; set; } = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();

            public FizzOptions Fizzes { get; set; } = new FizzOptions();
            public BuzzOptions Buzz { get; set; } = new BuzzOptions();
            public FooOptions Foos { get; set; } = new FooOptions();
            public BarOptions Bar { get; set; } = new BarOptions();
            public FrobOptions Frob { get; set; } = new FrobOptions();
        }

        public class FrobOptions : FeatureToggle
        {
            public bool A { get; set; } = false;
            public string B { get; set; } = "0";
            public string C { get; set; } = "Foo";
            public string D { get; set; } = "Foo";
            public int? E { get; set; } = null;
            public Options F { get; set; } = Options.A;
        }

        public enum Options
        {
            A,
            B,
            C
        }

        public class BarOptions : FeatureToggle
        {
            public string Value { get; set; } = "Foo";
        }

        public class FooOptions : FeatureToggle
        {
            public string Value { get; set; } = "Foo";

            public string[] Values { get; set; } =
                {"A", "B", "C"};
        }

        public class FizzOptions : FeatureToggle
        {
            public long Number { get; set; } = 30_000_000;
        }

        public class BuzzOptions : FeatureToggle
        {
            public string Value { get; set; } = "Foo";

            public string C { get; set; } = "Foo";
            public bool CBool { get; set; } = false;

            public string B { get; set; } = "Foo";
            public bool BBool { get; set; } = true;

            public string A { get; set; } = "Foo";
            public bool ABool { get; set; } = true;
        }

        #endregion
    }
}