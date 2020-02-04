// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace TypeKitchen.Composition.Internal
{
	internal static class EnumerableExtensions
	{
		public static List<T> TopologicalSort<T>(this IReadOnlyCollection<T> collection, Func<T, IEnumerable<T>> getDependentsFunc) where T : IEquatable<T>
		{
			var edges = new List<Tuple<T, T>>();
			foreach (var item in collection)
			{
				var dependents = getDependentsFunc(item);
				foreach (var dependent in dependents)
				{
					edges.Add(new Tuple<T, T>(item, dependent));
				}
			}
			var sorted = TopologicalSorter<T>.Sort(collection, edges);
			return sorted;
		}
	}
}
