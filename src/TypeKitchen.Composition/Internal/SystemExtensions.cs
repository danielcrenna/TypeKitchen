// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeKitchen.Internal;

namespace TypeKitchen.Composition.Internal
{
	internal static class SystemExtensions
	{
		public static IEnumerable<(Type, Value128)> Archetypes(this ISystem system, Value128 seed = default)
		{
			var type = system.GetType();
			var implemented = type.GetTypeInfo().ImplementedInterfaces;
			foreach (var contract in implemented.Where(x => x.IsGenericType && typeof(ISystem).IsAssignableFrom(x)))
			{
				IEnumerable<Type> componentTypes = contract.GetGenericArguments();
				if (typeof(ISystemWithState).IsAssignableFrom(contract))
					componentTypes = componentTypes.Skip(1);

				var hash = componentTypes.Archetype(seed);
				yield return (contract, hash);
			}
		}
	}
}
