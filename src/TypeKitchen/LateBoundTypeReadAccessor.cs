// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace TypeKitchen
{
	internal sealed class LateBoundTypeReadAccessor : ITypeReadAccessor
	{
		private readonly IDictionary<string, Func<object, object>> _binding;
		
		public LateBoundTypeReadAccessor(AccessorMembers members)
		{
			Type = members.DeclaringType;
			_binding = LateBinding.DynamicMethodBindGet(members);
		}

		public object this[object target, string key] => _binding[key](target);

		public bool TryGetValue(object target, string key, out object value)
		{
			var bound = _binding.TryGetValue(key, out var getter);
			value = bound ? getter(target) : default;
			return bound;
		}

		public Type Type { get; }
	}
}