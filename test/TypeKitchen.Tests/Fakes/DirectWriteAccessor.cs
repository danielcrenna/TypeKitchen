// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Tests.Fakes
{
	public sealed class DirectWriteAccessor : ITypeWriteAccessor
	{
		public static DirectWriteAccessor Instance = new DirectWriteAccessor();

		private DirectWriteAccessor()
		{
		}

		public Type Type => typeof(OnePropertyOneFieldStrings);

		public bool TrySetValue(object target, string key, object value)
		{
			switch (key)
			{
				case "Foo":
					((OnePropertyOneFieldStrings) target).Foo = (string) value;
					return true;
				case "Bar":
					((OnePropertyOneFieldStrings) target).Bar = (string) value;
					return true;
				default:
					return false;
			}
		}

		public object this[object target, string key]
		{
			set
			{
				switch (key)
				{
					case "Foo":
						((OnePropertyOneFieldStrings) target).Foo = (string) value;
						return;
					case "Bar":
						((OnePropertyOneFieldStrings) target).Bar = (string) value;
						return;
					default:
						throw new ArgumentNullException();
				}
			}
		}
	}
}