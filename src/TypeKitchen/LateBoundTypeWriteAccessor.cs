// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace TypeKitchen
{
	internal sealed class LateBoundTypeWriteAccessor : ITypeWriteAccessor
	{
		private readonly IDictionary<string, Action<object, object>> _binding;
		
		public LateBoundTypeWriteAccessor(AccessorMembers members)
		{
			Type = members.DeclaringType;
			_binding = LateBinding.DynamicMethodBindSet(members);
		}

		public object this[object target, string key]
		{
			set => _binding[key](target, value);
		}

		public bool TrySetValue(object target, string key, object value)
		{
			var bound = _binding.TryGetValue(key, out var setter);
			if (bound)
				setter(target, value);
			return bound;
		}

		public Type Type { get; }
	}
}