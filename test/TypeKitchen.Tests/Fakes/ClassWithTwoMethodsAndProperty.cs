// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen.Tests.Fakes
{
	public class ClassWithTwoMethodsAndProperty : ITwoMethodsAndProperty
	{
		public int Count;

		public ClassWithTwoMethodsAndProperty() { }

		public ClassWithTwoMethodsAndProperty(int i) => Count = i;

		public void Foo()
		{
			Count++;
		}

		public void Bar(int i)
		{
			Count += i;
		}

		public string Baz => "ABC";

		public static int Method() { return 1; }
	}
}