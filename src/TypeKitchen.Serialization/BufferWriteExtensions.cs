// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace TypeKitchen.Serialization
{
	public static class BufferWriteExtensions
	{
		private static readonly Encoding Encoding = Encoding.UTF8;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteString(this ref Span<byte> buffer, ref int offset, StringValues value)
		{
			var stringValue = value.ToString(); // zero-alloc if single value

			if (value.Count <= 0)
			{
				buffer.MaybeResize(offset, 1);
				buffer.WriteBoolean(ref offset, value.Count > 0);
				return;
			}

			var charCount = stringValue.Length;
			var byteCount = Encoding.GetByteCount(stringValue);

			buffer.MaybeResize(offset, 1 + sizeof(int) + byteCount);
			buffer.WriteBoolean(ref offset, true);
			buffer.WriteInt32(ref offset, byteCount);

			unsafe
			{
				fixed (char* source = &stringValue.AsSpan().GetPinnableReference())
				fixed (byte* target = &buffer.Slice(offset, byteCount).GetPinnableReference())
					offset += Encoding.GetBytes(source, charCount, target, byteCount);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteBoolean(this ref Span<byte> buffer, ref int offset, bool value)
		{
			buffer[offset] = (byte) (value ? 1 : 0);
			offset++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt32(this ref Span<byte> buffer, ref int offset, int value)
		{
			unsafe
			{
				fixed (byte* b = &buffer.GetPinnableReference()) *(int*) (b + offset) = value;
			}

			offset += sizeof(int);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MaybeResize(this ref Span<byte> buffer, long offset, int nextWriteLength)
		{
			var length = offset + nextWriteLength;
			if (buffer.Length >= length) return;

			var allocate = new byte[length];
			buffer.TryCopyTo(allocate);
			buffer = allocate.AsSpan();
		}
	}
}