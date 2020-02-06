// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Differencing
{
	public interface IValueHashProvider
	{
		ulong ComputeHash64(ReadOnlySpan<byte> buffer, ulong seed);
	}
}