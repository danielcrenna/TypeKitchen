// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace ExternalTestAssembly
{
	public static class AnonymousTypeFactory
	{
		public static object Foo()
		{
			return new {Foo = "Foo"};
		}

		public static object Bar()
		{
			return new {Bar = "Bar"};
		}
	}
}