// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace TypeKitchen.Tests.Pooling
{
	public class StopwatchPoolTests
	{
		[Fact]
		public void Can_use_scoped_builder()
		{
			var elapsed = TypeKitchen.Pooling.StopwatchPool.Scoped(x =>
			{
				Task.Delay(100).Wait();
			});

			Assert.NotEqual(default, elapsed);
			Assert.True(elapsed.TotalMilliseconds >= 100, elapsed.ToString());
		}
	}
}