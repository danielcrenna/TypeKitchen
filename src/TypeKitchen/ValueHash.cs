using System;
using WyHash;

namespace TypeKitchen
{
	public static class ValueHash
	{
		static ValueHash() =>
			Seed = BitConverter.ToUInt64(
				new[] { (byte) 'd', (byte) 'e', (byte) 'a', (byte) 'd', (byte) 'b', (byte) 'e', (byte) 'e', (byte) 'f' },
				0);

		public static ulong Seed { get; set; }

		public static ulong ComputeHash(object instance, ulong? seed = null)
		{
			var buffer = Wire.Simple(instance);
			if (buffer == null)
				return default;
			return WyHash64.ComputeHash64(buffer, seed ?? Seed);
		}
	}
}