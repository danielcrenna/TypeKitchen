// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using TypeKitchen.Serialization;

namespace TypeKitchen.Tests.Serialization
{
	namespace V1
	{
		public ref struct PersonFlyweight
		{
			private readonly ReadOnlySpan<byte> _buffer;

			public string Name => _buffer.ReadString(0);

			public PersonFlyweight(ReadOnlySpan<byte> buffer) => _buffer = buffer;
		}
	}

	namespace V2
	{
		public ref struct PersonFlyweight
		{
			private readonly ReadOnlySpan<byte> _buffer;

			public string FirstName => _buffer.ReadString(0);

			public string LastName
			{
				get
				{
					var offset = 1;
					if (_buffer.ReadBoolean(0)) offset += sizeof(int) + _buffer.ReadInt32(1);

					return _buffer.ReadString(offset);
				}
			}

			public PersonFlyweight(ReadOnlySpan<byte> buffer) => _buffer = buffer;
		}
	}
}