using System;
using System.IO;

namespace TypeKitchen
{
    // FIXME: converge with serialization extension methods

	internal static class BuiltIns
	{
		public static bool ReadBoolean(this BinaryReader br)
		{
			return br.ReadBoolean();
		}

		public static bool WriteBoolean(this BinaryWriter bw, bool value)
		{
			bw.Write(value);
			return value;
		}

		#region TimeSpan

		public static void WriteTimeSpan(this BinaryWriter bw, TimeSpan value)
		{
			bw.Write(value.Ticks);
		}

		public static TimeSpan ReadTimeSpan(this BinaryReader br)
		{
			return TimeSpan.FromTicks(br.ReadInt64());
		}

		#endregion

		#region DateTimeOffset

		public static void WriteDateTimeOffset(this BinaryWriter bw, DateTimeOffset value)
		{
			bw.WriteTimeSpan(value.Offset);
			bw.Write(value.Ticks);
		}

		public static object ReadDateTimeOffset(this BinaryReader br)
		{
			var offset = br.ReadTimeSpan();
			var ticks = br.ReadInt64();
			return new DateTimeOffset(new DateTime(ticks), offset);
		}

		#endregion
	}
}