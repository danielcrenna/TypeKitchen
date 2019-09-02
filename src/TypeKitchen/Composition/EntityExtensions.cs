// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen.Composition
{
	public static class EntityExtensions
	{
		public static void Set<T>(this uint entity, T value, Container container) where T : struct
		{
			container.SetComponent(entity, value);
		}
	}
}