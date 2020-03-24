// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using TypeKitchen.Serialization;

namespace TypeKitchen.Differencing
{
	public static class ValueHash
	{
		private static readonly ulong DefaultSeed;
		
		static ValueHash() => DefaultSeed = BitConverter.ToUInt64(new[] {(byte) 'd', (byte) 'e', (byte) 'a', (byte) 'd', (byte) 'b', (byte) 'e', (byte) 'e', (byte) 'f' }, 0);
		
		public static ulong ComputeHash(object instance, IObjectSerializer objectSerializer = null, IStringSerializer stringSerializer = null, ITypeResolver typeResolver = null, IValueHashProvider valueHashProvider = null, ulong? seed = null)
		{
			stringSerializer ??= Defaults.StringSerializer;
			objectSerializer ??= Defaults.ObjectSerializer;
			typeResolver ??= Defaults.TypeResolver;

			return instance is string text
				? ComputeHash(stringSerializer.ToBuffer(text, objectSerializer, typeResolver), valueHashProvider, seed)
				: ComputeHash(objectSerializer.ToBuffer(instance, typeResolver), valueHashProvider, seed);
		}

		public static ulong ComputeHash(ReadOnlySpan<byte> buffer, IValueHashProvider valueHashProvider = null, ulong? seed = null) => buffer == null ? default : (valueHashProvider ?? Defaults.ValueHashProvider).ComputeHash64(buffer, seed ?? DefaultSeed);
	}
}