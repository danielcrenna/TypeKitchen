// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TypeKitchen.Composition
{
	internal static class Combinations
	{
		static Combinations()
		{
			for(var i = 0; i < 8; i++)
				GetIndices(i + 1);
		}

		public static IEnumerable<IEnumerable<T>> GetCombinations<T>(this IList<T> values, int k = 0)
		{
			if (k < 0 || values == null)
				yield break;

			if (k == 0 || values.Count < k)
				k = values.Count;

			var indices = GetIndices(k);
			try
			{
				while (true)
				{
					yield return YieldCombination(values, indices, k);

					var n = k - 1;
					while (n >= 0 && indices[n] >= values.Count - k + n)
						n--;
					if (n < 0)
						yield break;

					for (var j = indices[n] + 1; n < k; n++, j++)
						indices[n] = j;
				}
			}
			finally
			{
				//Pool.Return(indices);
			}
		}

		private static IEnumerable<T> YieldCombination<T>(IList<T> values, IReadOnlyList<int> indices, int k)
		{
			for (var i = 0; i < k; i++)
				yield return values[indices[i]];
		}

		private static readonly ConcurrentDictionary<int, int[]> Indices = new ConcurrentDictionary<int, int[]>();

		private static readonly ArrayPool<int> Pool = ArrayPool<int>.Create();

		private static int[] GetIndices(int k)
		{
			return InitializeIndices(k);

			var prototype = Indices.GetOrAdd(k, InitializeIndices);
			var set = Pool.Rent(k);
			unsafe
			{
				var span = new ReadOnlySpan<int>(prototype);
				fixed (void* s = &span.GetPinnableReference())
				{
					fixed (void* d = &span.GetPinnableReference())
					{
						Unsafe.CopyBlock(d, s, (uint) k);
						return set;
					}
				}
			}
		}

		private static int[] InitializeIndices(int k)
		{
			var indices = new int[k];
			for (var i = 0; i < k; i++) indices[i] = i;
			return indices;
		}
	}
}