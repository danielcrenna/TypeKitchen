// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Tests.Fakes
{
	public sealed class DirectReadAccessorForOnePropertyOneField : ITypeReadAccessor
	{
		public static DirectReadAccessorForOnePropertyOneField
			Instance = new DirectReadAccessorForOnePropertyOneField();

		private DirectReadAccessorForOnePropertyOneField()
		{
		}

		public bool TryGetValue(object target, string key, out object value)
		{
			switch (key)
			{
				case "Foo":
					value = ((OnePropertyOneFieldStrings) target).Foo;
					return true;
				case "Bar":
					value = ((OnePropertyOneFieldStrings) target).Bar;
					return true;
				default:
					value = null;
					return false;
			}
		}

		public Type Type => typeof(OnePropertyOneFieldStrings);

		public object this[object target, string key]
		{
			get
			{
				switch (key)
				{
					case "Foo":
						return ((OnePropertyOneFieldStrings) target).Foo;
					case "Bar":
						return ((OnePropertyOneFieldStrings) target).Bar;
					default:
						throw new ArgumentNullException();
				}
			}
		}
	}
}