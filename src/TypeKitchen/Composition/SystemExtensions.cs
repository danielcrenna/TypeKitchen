// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeKitchen.Internal;

namespace TypeKitchen.Composition
{
	internal static class SystemExtensions
	{
		public static Value128 Archetype(this ISystem system, Value128 seed = default) =>
			system.GetDeclaredSystemComponentTypes().Archetype(seed);

		private static IEnumerable<Type> GetDeclaredSystemComponentTypes<T>(this T system) where T : ISystem
		{
			var implemented = system.GetType().GetTypeInfo().ImplementedInterfaces;
			var contract = implemented
				.Single(x => typeof(ISystem).IsAssignableFrom(x) && x.IsGenericType);

			IEnumerable<Type> componentTypes = contract.GetGenericArguments();
			if (typeof(ISystemWithState).IsAssignableFrom(contract))
				componentTypes = componentTypes.Skip(1);
			
			foreach (var argument in componentTypes)
				yield return argument;
		}
	}
}
