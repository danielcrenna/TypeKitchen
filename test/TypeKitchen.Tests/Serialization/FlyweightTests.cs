// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using TypeKitchen.Serialization;
using TypeKitchen.Tests.Serialization.Fakes.V2;
using Xunit;

namespace TypeKitchen.Tests.Serialization
{
	public class FlyweightTests
	{
		[Fact]
		public void ReadTests_SameVersion()
		{
			var person = new Person {FirstName = "Kawhi", LastName = "Leonard"};

			var ms = new MemoryStream();
			person.Serialize(ms);

			var span = ms.GetBuffer().AsSpan();
			var mirror = new PersonFlyweight(span);
			Assert.Equal(person.FirstName, mirror.FirstName);
			Assert.Equal(person.LastName, mirror.LastName);
		}

		[Fact]
		public void ReadTests_NextVersion()
		{
			var person = new Fakes.V1.Person {Name = "Kawhi"};

			var buffer = new byte[person.BufferSize];
			var ms = new MemoryStream(buffer);
			person.Serialize(ms);
			Assert.Equal(ms.Length, buffer.Length);

			var span = buffer.AsSpan();
			var mirror = new PersonFlyweight(span);
			Assert.Equal(person.Name, mirror.FirstName);
		}

		[Fact]
		public void ReadTests_PreviousVersion()
		{
			var person = new Person {FirstName = "Kawhi", LastName = "Leonard"};

			var buffer = new byte[person.BufferSize];
			var ms = new MemoryStream(buffer);
			person.Serialize(ms);
			Assert.Equal(ms.Length, buffer.Length);

			var span = buffer.AsSpan();
			var mirror = new Fakes.V1.PersonFlyweight(span);
			Assert.Equal(person.FirstName, mirror.Name);
		}

		[Fact]
		public void WriteTests_SameVersion_MultipleRows()
		{
			var person1 = new Fakes.V1.Person {Name = "Kawhi"};
			var person2 = new Fakes.V1.Person {Name = "Kyle"};

			var buffer = new byte[person1.BufferSize + person2.BufferSize];
			var ms = new MemoryStream(buffer);
			person1.Serialize(ms);
			person2.Serialize(ms);
			Assert.Equal(ms.Length, buffer.Length);

			var span = buffer.AsSpan();

			var row1 = new Fakes.V1.PersonFlyweight(span);
			Assert.Equal(person1.Name, row1.Name);

			var row2 = new Fakes.V1.PersonFlyweight(span.Slice(person1.BufferSize));
			Assert.Equal(person2.Name, row2.Name);
		}

		[Fact]
		public void WriteTests_DifferentVersions_MultipleRows()
		{
			var person1 = new Fakes.V1.Person {Name = "Kawhi"};
			var person2 = new Person {FirstName = "Kyle", LastName = "Lowry"};

			var buffer = new byte[person1.BufferSize + person2.BufferSize];
			var ms = new MemoryStream(buffer);
			person1.Serialize(ms);
			person2.Serialize(ms);
			Assert.Equal(ms.Length, buffer.Length);

			var span = buffer.AsSpan();

			var row11 = new Fakes.V1.PersonFlyweight(span);
			Assert.Equal(person1.Name, row11.Name);

			var row12 = new PersonFlyweight(span);
			Assert.Equal(person1.Name, row12.FirstName);

			var row21 = new Fakes.V1.PersonFlyweight(span.Slice(person1.BufferSize));
			Assert.Equal(person2.FirstName, row21.Name);

			var row22 = new PersonFlyweight(span.Slice(person1.BufferSize));
			Assert.Equal(person2.FirstName, row22.FirstName);
			Assert.Equal(person2.LastName, row22.LastName);
		}
	}
}