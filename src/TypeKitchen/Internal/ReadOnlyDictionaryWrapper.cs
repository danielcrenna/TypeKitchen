// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TypeKitchen.Internal
{
	internal struct ReadOnlyDictionaryWrapper : IReadOnlyDictionary<string, object>
	{
		private readonly ITypeReadAccessor _accessor;
		private readonly AccessorMembers _members;
		private readonly object _instance;

		public ReadOnlyDictionaryWrapper(ITypeReadAccessor accessor, object instance)
		{
			_accessor = accessor;
			_instance = instance;
			_members = accessor.Type.IsAnonymous()
				? AccessorMembers.Create(accessor.Type, AccessorMemberTypes.Properties, AccessorMemberScope.Public)
				: AccessorMembers.Create(accessor.Type,
					AccessorMemberTypes.Properties | AccessorMemberTypes.Fields);
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return Yield().GetEnumerator();
		}

		private IEnumerable<KeyValuePair<string, object>> Yield()
		{
			foreach (var member in _members)
				yield return new KeyValuePair<string, object>(member.Name, _accessor[_instance, member.Name]);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count => _members.Count;

		public bool ContainsKey(string key)
		{
			return _members.ContainsKey(key);
		}

		public bool TryGetValue(string key, out object value)
		{
			return _accessor.TryGetValue(_instance, key, out value);
		}

		public object this[string key] => _accessor[_instance, key];
		public IEnumerable<string> Keys => _members.Names;

		public IEnumerable<object> Values => YieldValues();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IEnumerable<object> YieldValues()
		{
			foreach (var member in _members)
				yield return _accessor[_instance, member.Name];
		}
	}
}