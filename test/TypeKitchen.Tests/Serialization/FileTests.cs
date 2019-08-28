// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.Serialization;
using TypeKitchen.Tests.Serialization.V1;
using Xunit;

namespace TypeKitchen.Tests.Serialization
{
	public class FileTests
	{
		[Fact]
		public void WriteTests_Simple()
		{
			using (var fixture = new TemporaryFileFixture())
			{
				var person = new Person {Name = "Kawhi"};

				person.Serialize(fixture.FileStream);
			}
		}
	}
}