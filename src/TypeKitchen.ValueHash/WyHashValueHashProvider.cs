// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using WyHash;

namespace TypeKitchen.ValueHash
{
	public class WyHashValueHashProvider : IValueHashProvider
	{
		public ulong ComputeHash64(ReadOnlySpan<byte> buffer, ulong seed) => WyHash64.ComputeHash64(buffer, seed);
	}
}