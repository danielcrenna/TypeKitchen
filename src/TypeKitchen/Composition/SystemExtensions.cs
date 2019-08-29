using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TypeKitchen.Composition
{
	internal static class SystemExtensions
	{
		public static Value128 Archetype(this ISystem system, Value128 seed = default) =>
			system.GetDeclaredSystemComponentTypes().Archetype(seed);

		private static IEnumerable<Type> GetDeclaredSystemComponentTypes<T>(this T system) where T : ISystem
		{
			var implemented = system.GetType().GetTypeInfo().ImplementedInterfaces;
			return implemented
				.Single(x => typeof(ISystem).IsAssignableFrom(x) && x.IsGenericType)
				.GetGenericArguments();
		}
	}
}
