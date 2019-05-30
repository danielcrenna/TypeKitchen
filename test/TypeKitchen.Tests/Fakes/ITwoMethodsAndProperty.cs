// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen.Tests.Fakes
{
	public interface ITwoMethodsAndProperty
	{
		string Baz { get; }
		void Foo();
		void Bar(int i);
	}
}