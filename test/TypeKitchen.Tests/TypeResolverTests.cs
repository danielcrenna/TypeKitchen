// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace TypeKitchen.Tests
{
	public class TypeResolverTests
	{
		[Fact]
		public void Find_by_interface()
		{
			var resolver = new ReflectionTypeResolver();
			var disposables = resolver.FindByInterface<IDisposable>().ToList();
			Assert.NotEmpty(disposables);
			Assert.All(disposables, t => Assert.True(typeof(IDisposable).IsAssignableFrom(t)));
		}
	}
}
