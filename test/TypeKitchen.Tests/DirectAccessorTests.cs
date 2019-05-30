// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
	public class DirectAccessorTests
	{
		[Fact]
		public void Can_create_delegate_for_indexer()
		{
			var accessor = DirectAnonymousReadAccessor.Instance;
			var fake = new TwoProperties {Foo = "Bar", Bar = "Baz"};
			var getFoo = accessor.SimulateNonBranchingIndirectAccess("Foo");
			var getBar = accessor.SimulateNonBranchingIndirectAccess("Bar");
			Assert.Equal("Bar", getFoo(fake));
			Assert.Equal("Baz", getBar(fake));
		}
	}
}