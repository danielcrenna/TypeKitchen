// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.Differencing;
using Xunit;

namespace TypeKitchen.Tests.Differencing
{
	public class ValueHashTests
	{
		[Fact]
		public void Order_does_not_matter()
		{
			var a = "{ \"foo\":\"bar\", \"bar\":\"baz\" }";
			var b = "{ \"bar\":\"baz\", \"foo\":\"bar\" }";

			var ah = ValueHash.ComputeHash(a);
			var bh = ValueHash.ComputeHash(b);
			Assert.Equal(ah, bh);
		}
	}
}