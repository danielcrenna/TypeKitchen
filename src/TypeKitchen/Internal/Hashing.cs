// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace TypeKitchen.Internal
{
	internal class Hashing
	{
		private const ulong C1 = 0x87c37b91114253d5u;
		private const ulong C2 = 0x4cf5ad432745937fu;

		public static Value128 MurmurHash3(byte[] key, Value128 seed = default)
		{
			var len = key.Length;
			var blocks = len / 16;

			var h1 = seed.v1;
			var h2 = seed.v2;

			//----------
			// body
			for (var i = 0; i < blocks; i++)
			{
				// Get 128 bits from key
				ulong k1 = key[i * 16];
				k1 |= (ulong) key[i * 16 + 1] << 8;
				k1 |= (ulong) key[i * 16 + 2] << 16;
				k1 |= (ulong) key[i * 16 + 3] << 24;
				k1 |= (ulong) key[i * 16 + 4] << 32;
				k1 |= (ulong) key[i * 16 + 5] << 40;
				k1 |= (ulong) key[i * 16 + 6] << 48;
				k1 |= (ulong) key[i * 16 + 7] << 56;

				ulong k2 = key[i * 16 + 8];
				k2 |= (ulong) key[i * 16 + 9] << 8;
				k2 |= (ulong) key[i * 16 + 10] << 16;
				k2 |= (ulong) key[i * 16 + 11] << 24;
				k2 |= (ulong) key[i * 16 + 12] << 32;
				k2 |= (ulong) key[i * 16 + 13] << 40;
				k2 |= (ulong) key[i * 16 + 14] << 48;
				k2 |= (ulong) key[i * 16 + 15] << 56;

				k1 *= C1;
				k1 = Rotl64(k1, 31);
				k1 *= C2;
				h1 ^= k1;

				h1 = Rotl64(h1, 27);
				h1 += h2;
				h1 = h1 * 5 + 0x52dce729;

				k2 *= C2;
				k2 = Rotl64(k2, 33);
				k2 *= C1;
				h2 ^= k2;

				h2 = Rotl64(h2, 31);
				h2 += h1;
				h2 = h2 * 5 + 0x38495ab5;
			}

			//----------
			// tail
			{
				ulong k1 = 0;
				ulong k2 = 0;

				var offset = blocks * 16;

				switch (len & ((1u << 4) - 1)) // len & 15
				{
					case 15:
						k2 |= (ulong) key[offset + 14] << 48;
						goto case 14;
					case 14:
						k2 |= (ulong) key[offset + 13] << 40;
						goto case 13;
					case 13:
						k2 |= (ulong) key[offset + 12] << 32;
						goto case 12;
					case 12:
						k2 |= (ulong) key[offset + 11] << 24;
						goto case 11;
					case 11:
						k2 |= (ulong) key[offset + 10] << 16;
						goto case 10;
					case 10:
						k2 |= (ulong) key[offset + 9] << 8;
						goto case 9;
					case 9:
						k2 ^= (ulong) key[8] << 0;
						k2 *= C2;
						k2 = Rotl64(k2, 33);
						k2 *= C1;
						h2 ^= k2;
						goto case 8;

					case 8:
						k1 |= (ulong) key[offset + 7] << 56;
						goto case 7;
					case 7:
						k1 |= (ulong) key[offset + 6] << 48;
						goto case 6;
					case 6:
						k1 |= (ulong) key[offset + 5] << 40;
						goto case 5;
					case 5:
						k1 |= (ulong) key[offset + 4] << 32;
						goto case 4;
					case 4:
						k1 |= (ulong) key[offset + 3] << 24;
						goto case 3;
					case 3:
						k1 |= (ulong) key[offset + 2] << 16;
						goto case 2;
					case 2:
						k1 |= (ulong) key[offset + 1] << 8;
						goto case 1;
					case 1:
						k1 ^= (ulong) key[0] << 0;
						k1 *= C1;
						k1 = Rotl64(k1, 31);
						k1 *= C2;
						h1 ^= k1;
						break;
				}
			}

			//----------
			// finalization

			h1 ^= (ulong) len;
			h2 ^= (ulong) len;

			h1 += h2;
			h2 += h1;

			h1 = Fmix64(h1);
			h2 = Fmix64(h2);

			h1 += h2;
			h2 += h1;

			var retval = new Value128();
			retval.v1 = h1;
			retval.v2 = h2;
			return retval;
		}

		public static Value128 MurmurHash3(string key, Value128 seed = default)
		{
			var len = key.Length * 2; // chars are 16 bits
			var blocks = key.Length / 8;

			var h1 = seed.v1;
			var h2 = seed.v2;

			//----------
			// body
			for (var i = 0; i < blocks; i++)
			{
				// Get 128 bits from key
				ulong k1 = key[i * 8];
				k1 |= (ulong) key[i * 8 + 1] << 16;
				k1 |= (ulong) key[i * 8 + 2] << 32;
				k1 |= (ulong) key[i * 8 + 3] << 48;

				ulong k2 = key[i * 8 + 4];
				k2 |= (ulong) key[i * 8 + 5] << 16;
				k2 |= (ulong) key[i * 8 + 6] << 32;
				k2 |= (ulong) key[i * 8 + 7] << 48;

				k1 *= C1;
				k1 = Rotl64(k1, 31);
				k1 *= C2;
				h1 ^= k1;

				h1 = Rotl64(h1, 27);
				h1 += h2;
				h1 = h1 * 5 + 0x52dce729;

				k2 *= C2;
				k2 = Rotl64(k2, 33);
				k2 *= C1;
				h2 ^= k2;

				h2 = Rotl64(h2, 31);
				h2 += h1;
				h2 = h2 * 5 + 0x38495ab5;
			}

			//----------
			// tail
			{
				ulong k1 = 0;
				ulong k2 = 0;

				var offset = blocks * 8;

				switch (key.Length & 7) // len & 15
				{
					case 7:
						k2 |= (ulong) key[offset + 6] << 32;
						goto case 6;
					case 6:
						k2 |= (ulong) key[offset + 5] << 16;
						goto case 5;
					case 5:
						k2 |= (ulong) key[offset + 4] << 0;
						k2 *= C2;
						k2 = Rotl64(k2, 33);
						k2 *= C1;
						h2 ^= k2;
						goto case 4;

					case 4:
						k1 |= (ulong) key[offset + 3] << 48;
						goto case 3;
					case 3:
						k1 |= (ulong) key[offset + 2] << 32;
						goto case 2;
					case 2:
						k1 |= (ulong) key[offset + 1] << 16;
						goto case 1;
					case 1:
						k1 |= (ulong) key[offset + 0] << 0;
						k1 *= C1;
						k1 = Rotl64(k1, 31);
						k1 *= C2;
						h1 ^= k1;
						break;
				}
			}

			//----------
			// finalization

			h1 ^= (ulong) len;
			h2 ^= (ulong) len;

			h1 += h2;
			h2 += h1;

			h1 = Fmix64(h1);
			h2 = Fmix64(h2);

			h1 += h2;
			h2 += h1;

			var retval = new Value128();
			retval.v1 = h1;
			retval.v2 = h2;
			return retval;
		}

		public static Value128 MurmurHash3(StringBuilder key, Value128 seed = default)
		{
			var len = key.Length * 2; // chars are 16 bits
			var blocks = key.Length / 8;

			var h1 = seed.v1;
			var h2 = seed.v2;

			//----------
			// body
			for (var i = 0; i < blocks; i++)
			{
				// Get 128 bits from key
				ulong k1 = key[i * 8];
				k1 |= (ulong) key[i * 8 + 1] << 16;
				k1 |= (ulong) key[i * 8 + 2] << 32;
				k1 |= (ulong) key[i * 8 + 3] << 48;

				ulong k2 = key[i * 8 + 4];
				k2 |= (ulong) key[i * 8 + 5] << 16;
				k2 |= (ulong) key[i * 8 + 6] << 32;
				k2 |= (ulong) key[i * 8 + 7] << 48;

				k1 *= C1;
				k1 = Rotl64(k1, 31);
				k1 *= C2;
				h1 ^= k1;

				h1 = Rotl64(h1, 27);
				h1 += h2;
				h1 = h1 * 5 + 0x52dce729;

				k2 *= C2;
				k2 = Rotl64(k2, 33);
				k2 *= C1;
				h2 ^= k2;

				h2 = Rotl64(h2, 31);
				h2 += h1;
				h2 = h2 * 5 + 0x38495ab5;
			}

			//----------
			// tail
			{
				ulong k1 = 0;
				ulong k2 = 0;

				var offset = blocks * 8;

				switch (key.Length & 7) // len & 15
				{
					case 7:
						k2 |= (ulong) key[offset + 6] << 32;
						goto case 6;
					case 6:
						k2 |= (ulong) key[offset + 5] << 16;
						goto case 5;
					case 5:
						k2 |= (ulong) key[offset + 4] << 0;
						k2 *= C2;
						k2 = Rotl64(k2, 33);
						k2 *= C1;
						h2 ^= k2;
						goto case 4;

					case 4:
						k1 |= (ulong) key[offset + 3] << 48;
						goto case 3;
					case 3:
						k1 |= (ulong) key[offset + 2] << 32;
						goto case 2;
					case 2:
						k1 |= (ulong) key[offset + 1] << 16;
						goto case 1;
					case 1:
						k1 |= (ulong) key[offset + 0] << 0;
						k1 *= C1;
						k1 = Rotl64(k1, 31);
						k1 *= C2;
						h1 ^= k1;
						break;
				}
			}

			//----------
			// finalization

			h1 ^= (ulong) len;
			h2 ^= (ulong) len;

			h1 += h2;
			h2 += h1;

			h1 = Fmix64(h1);
			h2 = Fmix64(h2);

			h1 += h2;
			h2 += h1;

			var retval = new Value128();
			retval.v1 = h1;
			retval.v2 = h2;
			return retval;
		}

		public static Value128 MurmurHash3(ulong key, Value128 seed = default)
		{
			const int len = 4;

			var h1 = seed.v1;
			var h2 = seed.v2;

			//----------
			// tail
			{
				ulong k1 = 0;

				k1 = key;
				k1 *= C1;
				k1 = Rotl64(k1, 31);
				k1 *= C2;
				h1 ^= k1;
			}

			//----------
			// finalization

			h1 ^= len;
			h2 ^= len;

			h1 += h2;
			h2 += h1;

			h1 = Fmix64(h1);
			h2 = Fmix64(h2);

			h1 += h2;
			h2 += h1;

			var retval = new Value128();
			retval.v1 = h1;
			retval.v2 = h2;
			return retval;
		}

		private static ulong Rotl64(ulong x, int r)
		{
			return (x << r) | (x >> (64 - r));
		}

		private static ulong Fmix64(ulong k)
		{
			k ^= k >> 33;
			k *= 0xff51afd7ed558ccd;
			k ^= k >> 33;
			k *= 0xc4ceb9fe1a85ec53;
			k ^= k >> 33;
			return k;
		}
	}
}