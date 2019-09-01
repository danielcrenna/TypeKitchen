// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace TypeKitchen.Tests
{
	public class ProxyTests
	{
		private static readonly object Sync = new object();

		[Fact]
		public void Can_create_proxies_for_all_types()
		{
			//
			// Pure Proxy:
			var pureProxy = Proxy.Create(typeof(FooClass));
			Assert.NotNull(pureProxy);
			var pureInstance = Activator.CreateInstance(pureProxy);
			Assert.NotNull(pureInstance);

			//
			// Hybrid Proxy:
			var hybridProxy = Proxy.Create(typeof(FooClass), ProxyType.Hybrid);
			Assert.NotNull(hybridProxy);
			var hybridInstance = Activator.CreateInstance(hybridProxy);
			Assert.NotNull(hybridInstance);

			//
			// Mimic Reference Type:
			var sealedClassProxy = Proxy.Create(typeof(FooClassSealed), ProxyType.Mimic);
			Assert.NotNull(sealedClassProxy);
			var sealedClassInstance = Activator.CreateInstance(sealedClassProxy);
			Assert.NotNull(sealedClassInstance);

			//
			// Mimic Value Type:
			var structProxy = Proxy.Create(typeof(FooStruct), ProxyType.Mimic);
			Assert.NotNull(structProxy);
			var structInstance = Activator.CreateInstance(structProxy);
			Assert.NotNull(structInstance);
		}

		[Fact]
		public void Can_access_pure_proxy()
		{
			TestDescendent<FooClass>(ProxyType.Pure);
			Assert.Throws<InvalidOperationException>(() =>
			{
				TestDescendent<FooClassSealed>(ProxyType.Pure);	// sealed
				TestDescendent<FooStruct>(ProxyType.Pure);		// sealed
			});
		}

		[Fact]
		public void Can_access_hybrid_proxy()
		{
			TestDescendent<FooClass>(ProxyType.Hybrid);
			Assert.Throws<InvalidOperationException>(() =>
			{
				TestDescendent<FooClassSealed>(ProxyType.Hybrid); // sealed
				TestDescendent<FooStruct>(ProxyType.Hybrid);      // sealed
			});
		}

		[Fact]
		public void Can_access_mimic_proxy()
		{
			TestAntecedent<FooClass>(ProxyType.Mimic);
			TestAntecedent<FooClassSealed>(ProxyType.Mimic);
			TestAntecedent<FooStruct>(ProxyType.Mimic);
			TestDescendent<IFoo>(ProxyType.Mimic);
		}

		private static void TestDescendent<T>(ProxyType proxyType)
		{
			var type = Proxy.Create(typeof(T), proxyType);
			Assert.NotNull(type);

			object target = Activator.CreateInstance(type);
			Assert.NotNull(target);

			var write = WriteAccessor.Create(type);
			Assert.True(write.TrySetValue(target, nameof(FooClass.Bar), "Baz"));
			Assert.True(write.TrySetValue(target, nameof(FooClass.Baz), 123));

			var read = ReadAccessor.Create(type);
			Assert.True(read.TryGetValue(target, nameof(FooClass.Bar), out var value));
			Assert.Equal("Baz", value);
			Assert.True(read.TryGetValue(target, nameof(FooClass.Baz), out value));
			Assert.Equal(123, value);

			var direct = (T)target;
			Assert.NotNull(direct);
		}

		private static void TestAntecedent<T>(ProxyType proxyType)
		{
			var type = Proxy.Create(typeof(T), proxyType);
			Assert.NotNull(type);

			object target = Activator.CreateInstance(type);
			Assert.NotNull(target);

			var write = WriteAccessor.Create(type);
			Assert.True(write.TrySetValue(target, nameof(FooClass.Bar), "Baz"));
			Assert.True(write.TrySetValue(target, nameof(FooClass.Baz), 123));

			var read = ReadAccessor.Create(type);
			Assert.True(read.TryGetValue(target, nameof(FooClass.Bar), out var value));
			Assert.Equal("Baz", value);
			Assert.True(read.TryGetValue(target, nameof(FooClass.Baz), out value));
			Assert.Equal(123, value);

			Assert.Throws<InvalidCastException>(() =>
			{
				var direct = (T) target;
				Assert.Null(direct); // does not inherit
			});
		}

		public class FooClass
		{
			public string Bar { get; set; }
			public int Baz;
		}

		public interface IFoo
		{
			string Bar { get; set; }
			int Baz { get; set; }
		}

		public sealed class FooClassSealed
		{
			public string Bar { get; set; }
			public int Baz;
		}

		public struct FooStruct
		{
			public string Bar { get; set; }
			public int Baz;
		}
	}
}