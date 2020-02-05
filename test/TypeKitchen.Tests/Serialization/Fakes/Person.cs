// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace TypeKitchen.Tests.Serialization.Fakes
{
	namespace V1
	{
		public class Person
		{
			public string Name { get; set; }

			public int BufferSize => 1 + sizeof(int) + Encoding.UTF8.GetByteCount(Name);
		}
	}

	namespace V2
	{
		public class Person
		{
			public string FirstName { get; set; }
			public string LastName { get; set; }

			public int BufferSize =>
				1 + sizeof(int) + Encoding.UTF8.GetByteCount(FirstName) +
				1 + sizeof(int) + Encoding.UTF8.GetByteCount(LastName);
		}
	}
}