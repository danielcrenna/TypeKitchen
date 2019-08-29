// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace TypeKitchen.Composition
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Value128
	{
		public ulong v1, v2;

		public static bool operator ==(Value128 a, Value128 b)
		{
			return a.v1 == b.v1 && a.v2 == b.v2;
		}

		public static bool operator !=(Value128 a, Value128 b)
		{
			return !(a == b);
		}

		public static Value128 operator ^(Value128 a, Value128 b)
		{
			return new Value128 {v1 = a.v1 ^ b.v1, v2 = a.v2 ^ b.v2};
		}

		public static implicit operator Value128(string id)
		{
			return new Value128
			{
				v1 = Convert.ToUInt64(id.Substring(0, 16), 16), v2 = Convert.ToUInt64(id.Substring(16, 16), 16)
			};
		}

		#region Object guff

		public override bool Equals(object obj)
		{
			return obj is Value128 && (Value128) obj == this;
		}

		public override int GetHashCode()
		{
			return (int) (v1 ^ v2);
		}

		public override string ToString()
		{
			return string.Format("{0:X8}{1:X8}", v1, v2);
		}

		#endregion
	}
}