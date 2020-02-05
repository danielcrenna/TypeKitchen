// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Serialization
{
	public class WireDeserializer : IObjectDeserializer
	{
		public T BufferToObject<T>(ReadOnlySpan<byte> buffer, ITypeResolver typeResolver, IReadObjectSink emitter = null) => (T) BufferToObject(buffer, typeof(T), typeResolver, emitter);

		public object BufferToObject(ReadOnlySpan<byte> buffer, Type type, ITypeResolver typeResolver, IReadObjectSink emitter = null) => Wire.BufferToObject(buffer, type, typeResolver, emitter);
	}
}