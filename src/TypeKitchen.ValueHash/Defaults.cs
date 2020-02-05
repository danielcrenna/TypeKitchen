using TypeKitchen.Serialization;

namespace TypeKitchen.ValueHash
{
	internal class Defaults
	{
		public static readonly ITypeResolver TypeResolver = new ReflectionTypeResolver();
		public static readonly IValueHashProvider ValueHashProvider = new WyHashValueHashProvider();
		public static readonly IObjectSerializer ObjectSerializer = new WireSerializer();
		public static readonly IObjectDeserializer ObjectDeserializer = new WireDeserializer();
	}
}
