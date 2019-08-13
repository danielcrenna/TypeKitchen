// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
	[Flags]
	public enum AccessorMemberScope : byte
	{
		Public = 1 << 0,
		Private = 1 << 1,

		None = 0xFF,
		All = byte.MaxValue
	}
}