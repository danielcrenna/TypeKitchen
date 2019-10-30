// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
	[Flags]
	public enum AccessorMemberTypes : byte
	{
		Fields = 1 << 1,
		Properties = 1 << 2,
		Methods = 1 << 3,

		None = 0x00,
		All = byte.MaxValue
	}
}