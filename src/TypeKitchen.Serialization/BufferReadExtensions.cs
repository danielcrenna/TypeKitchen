// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace TypeKitchen.Serialization
{
	public static class BufferReadExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ReadString(this ReadOnlySpan<byte> buffer, int offset)
		{
			if (!ReadBoolean(buffer, offset)) return null;

			var length = ReadInt32(buffer, offset + 1);
			var sliced = buffer.Slice(offset + 1 + sizeof(int), length);

			unsafe
			{
				fixed (byte* b = sliced) return Encoding.UTF8.GetString(b, sliced.Length);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBoolean(this ReadOnlySpan<byte> buffer, int offset)
		{
			return buffer[offset] != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ReadInt32(this ReadOnlySpan<byte> buffer, int offset)
		{
			unsafe
			{
				fixed (byte* ptr = &buffer.GetPinnableReference()) return *(int*) (ptr + offset);
			}
		}
	}
}