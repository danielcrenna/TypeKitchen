// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.IO;
using Microsoft.Extensions.Primitives;

namespace TypeKitchen.Serialization
{
	public static class FlyweightSerializer
	{
		private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Create();

		public static void Serialize<T>(this T subject, Stream stream)
		{
			var accessor = ReadAccessor.Create(typeof(T), AccessorMemberTypes.Properties, out var members);

			var buffer = Pool.Rent(4096);
			try
			{
				var offset = 0;
				var span = buffer.AsSpan();

				foreach (var member in members)
				{
					if (!member.CanWrite)
						continue;

					var value = accessor[subject, member.Name];

					switch (value)
					{
						case string v:
							span.WriteString(ref offset, v);
							break;
						case StringValues v:
							span.WriteString(ref offset, v);
							break;
						case int v:
							span.WriteInt32(ref offset, v);
							break;
						case bool v:
							span.WriteBoolean(ref offset, v);
							break;
					}
				}

				stream.Write(buffer, 0, offset);
			}
			finally
			{
				Pool.Return(buffer);
			}
		}
	}
}