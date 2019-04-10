using System.Collections.Generic;
using TypeKitchen.Internal;

namespace TypeKitchen
{
    public static class ReadAccessorExtensions
    {
        public static IReadOnlyDictionary<string, object> AsReadOnlyDictionary(this ITypeReadAccessor accessor, object instance)
        {
            return new ReadOnlyDictionaryWrapper(accessor, instance);
        }
    }
}
