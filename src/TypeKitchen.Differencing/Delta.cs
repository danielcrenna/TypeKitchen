using System;

namespace TypeKitchen.Differencing
{
	public class Delta
	{
		public static IDeltaStream ObjectToObject(object from, object to) =>
			BufferToBuffer(
				from.GetType(), Defaults.ObjectSerializer.ToBuffer(from, Defaults.TypeResolver),
				to.GetType(), Defaults.ObjectSerializer.ToBuffer(to, Defaults.TypeResolver)
			);

		public static IDeltaStream BufferToBuffer(Type fromType, ReadOnlySpan<byte> from, Type toType, ReadOnlySpan<byte> to)
		{
			if(from.SequenceEqual(to))
				return DiffDocument.Empty;
			
			var diff = new DiffDocument();
			Defaults.ObjectDeserializer.BufferToObject(from, fromType, Defaults.TypeResolver, diff);
			Defaults.ObjectDeserializer.BufferToObject(to, toType, Defaults.TypeResolver, diff);
			return diff;
		}
	}
}
