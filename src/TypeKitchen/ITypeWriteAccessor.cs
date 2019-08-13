// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
	public interface ITypeWriteAccessor
	{
		Type Type { get; }
		object this[object target, string key] { set; }
		bool TrySetValue(object target, string key, object value);
	}
}