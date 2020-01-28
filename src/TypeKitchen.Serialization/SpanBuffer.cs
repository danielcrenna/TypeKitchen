// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TypeKitchen.Serialization
{
	public readonly ref struct SpanBuffer<T>
	{
		private readonly List<SpanRef<T>> _list;

		public SpanBuffer(int capacity) => _list = new List<SpanRef<T>>(capacity);

		public void Add(Span<byte> span)
		{
			var ptr = span.Length == 0 ? IntPtr.Zero : new IntPtr(span.GetPinnableReference());

			_list.Add(new SpanRef<T> {Handle = ptr, Length = span.Length});
		}

		public long Length
		{
			get
			{
				var length = 0;
				foreach (var item in _list) length += item.Length;

				return length;
			}
		}

		public bool IsEmpty => Length == 0;

		public static SpanBuffer<T> Empty => new SpanBuffer<T>();

		public unsafe void Clear()
		{
			foreach (var item in _list)
			{
				var span = new Span<T>(item.Handle.ToPointer(), item.Length);
				span.Clear();
			}
		}

		// ReSharper disable once InconsistentNaming
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public static bool operator !=(SpanBuffer<T> left, SpanBuffer<T> right)
		{
			return !(left == right);
		}

		public static bool operator ==(SpanBuffer<T> left, SpanBuffer<T> right)
		{
			if (left.Length != right.Length) return false;

			for (var i = 0; i < left.Length; i++)
			{
				var l = left._list[i];
				var r = right._list[i];
				if (l.Length != r.Length) return false;

				if (l.Handle != r.Handle) return false;
			}

			return true;
		}

		public override string ToString()
		{
			unsafe
			{
				var capture = _list;

				return Pooling.StringBuilderPool.Scoped(sb =>
				{
					for (var i = 0; i < capture.Count; i++)
					{
						if (i != 0) sb.Append(", ");

						var item = capture[i];
						var span = new Span<T>(item.Handle.ToPointer(), item.Length);

						sb.Append(span.ToString());
					}
				});
			}
		}

		public ref struct Enumerator
		{
			private readonly SpanBuffer<T> _buffer;
			private int _index;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal Enumerator(SpanBuffer<T> buffer)
			{
				_buffer = buffer;
				_index = -1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				var num = _index + 1;
				if (num >= _buffer.Length) return false;

				_index = num;
				return true;
			}

			public ref T Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get
				{
					unsafe
					{
						var innerIndex = 0;
						var outerIndex = _index;
						for (var i = 0; i < _buffer._list.Count; i++)
						{
							var list = _buffer._list[i];
							if (list.Length < outerIndex)
								outerIndex -= i;
							else
							{
								innerIndex = i;
								break;
							}
						}

						var spanRef = _buffer._list[outerIndex];
						var span = new Span<T>(spanRef.Handle.ToPointer(), spanRef.Length);
						return ref span[innerIndex];
					}
				}
			}
		}

		#region NotSupported

		/// <inheritdoc />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool Equals(object obj)
		{
			throw new NotSupportedException("NotSupported_CannotCallEqualsOnSpan");
		}

		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override int GetHashCode()
		{
			throw new NotSupportedException("NotSupported_CannotCallGetHashCodeOnSpan");
		}

		#endregion
	}
}