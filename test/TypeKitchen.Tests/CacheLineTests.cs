// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Abstractions;

namespace TypeKitchen.Tests
{
	public class CacheLineTests
	{
		private readonly ITestOutputHelper _console;

		public CacheLineTests(ITestOutputHelper console)
		{
			_console = console;
		}

		[Fact]
		public void Can_get_cache_line_size()
		{
			_console.WriteLine(CacheLine.Size.ToString());
		}
	}
}