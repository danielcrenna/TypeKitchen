// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace TypeKitchen.Composition
{
	internal class AtomicLong
	{
		private long _value;

		public AtomicLong()
		{
			Set(0);
		}

		public AtomicLong(long value)
		{
			Set(value);
		}

		public long Get()
		{
			return Interlocked.Read(ref _value);
		}

		public void Set(long value)
		{
			Interlocked.Exchange(ref _value, value);
		}

		public long AddAndGet(long amount)
		{
			Interlocked.Add(ref _value, amount);
			return Get();
		}

		public long IncrementAndGet()
		{
			Interlocked.Increment(ref _value);
			return Get();
		}

		public bool CompareAndSet(long expected, long updated)
		{
			if (Get() == expected)
			{
				Set(updated);
				return true;
			}

			return false;
		}

		/// <summary>
		///     Set to the given value and return the previous value
		/// </summary>
		public long GetAndSet(long value)
		{
			var previous = Get();
			Set(value);
			return previous;
		}

		public long GetAndAdd(long value)
		{
			var previous = Get();
			Interlocked.Add(ref _value, value);
			return previous;
		}

		public static implicit operator AtomicLong(long value)
		{
			return new AtomicLong(value);
		}

		public static implicit operator long(AtomicLong value)
		{
			return value.Get();
		}
	}
}