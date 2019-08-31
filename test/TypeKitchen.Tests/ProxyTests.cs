// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace TypeKitchen.Tests
{
	public class ProxyTests
	{
		[Fact]
		public void Can_create_vanilla_proxy()
		{
			var proxyType = Proxy.Create(typeof(Foo));
			Assert.NotNull(proxyType);

			var instance = Activator.CreateInstance(proxyType);
			Assert.NotNull(instance);

			var write = WriteAccessor.Create(proxyType);
			Assert.True(write.TrySetValue(instance, nameof(Foo.Bar), "Baz"));
			Assert.True(write.TrySetValue(instance, nameof(Foo.Baz), 123));

			var read = ReadAccessor.Create(proxyType);
			Assert.True(read.TryGetValue(instance, nameof(Foo.Bar), out var value));
			Assert.Equal("Baz", value);
			Assert.True(read.TryGetValue(instance, nameof(Foo.Baz), out value));
			Assert.Equal(123, value);
		}

		public class Foo
		{
			public string Bar { get; set; }
			public int Baz { get; set; }
		}
	}
}