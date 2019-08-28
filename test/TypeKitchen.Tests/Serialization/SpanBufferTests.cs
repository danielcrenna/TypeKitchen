#region LICENSE

// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#endregion

using System;
using TypeKitchen.Serialization;
using Xunit;

namespace TypeKitchen.Tests.Serialization
{
	public class SpanBufferTests
	{
		[Fact]
		public void Can_combine_two_spans_in_one_span_buffer()
		{
			var buffer = new SpanBuffer<byte>(2);

			var ao = 0;
			var a = new byte[0].AsSpan();
			a.WriteString(ref ao, "A");

			var bo = 0;
			var b = new byte[0].AsSpan();
			b.WriteString(ref bo, "B");

			buffer.Add(a);
			buffer.Add(b);

			var length = 1 + sizeof(int) + 1 + /* A */
			             1 + sizeof(int) + 1; /* B */

			Assert.Equal(length, buffer.Length);
			Assert.False(buffer.IsEmpty);
		}

		[Fact]
		public void Empty_is_empty()
		{
			var buffer = new SpanBuffer<byte>(2);

			var a = new byte[0].AsSpan();
			var b = new byte[0].AsSpan();

			buffer.Add(a);
			buffer.Add(b);

			Assert.True(buffer.IsEmpty);
		}
	}
}