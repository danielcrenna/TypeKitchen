// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
	[Flags]
	public enum AccessorMemberTypes : byte
	{
		Fields = 1 << 0,
		Properties = 1 << 1,
		Methods = 1 << 2,

		None = 0xFF,
		All = byte.MaxValue
	}
}