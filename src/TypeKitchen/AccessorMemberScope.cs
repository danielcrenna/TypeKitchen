// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
	[Flags]
	public enum AccessorMemberScope : byte
	{
		Public = 1 << 1,
		Private = 1 << 2,

		None = 0x00,
		All = byte.MaxValue
	}
}