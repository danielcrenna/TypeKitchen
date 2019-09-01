// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace TypeKitchen
{
	public static class Duck
	{
		public static T QuackLike<T>(this object instance)
		{
			if (instance == null)
				return default;

			var source = instance.GetType();
			var target = typeof(T);

			if(target.IsInterface)
				return Proxy<T>(target, instance, ProxyType.Mimic);

			if (source.IsValueType && target.IsValueType)
				return Marshal<T>(ref instance);

			return ShallowCopy<T>(instance, target);
		}

		private static T ShallowCopy<T>(object instance, Type target)
		{
			var clone = Activator.CreateInstance(target);
			ShallowCopy(ref instance, ref clone);
			return (T) clone;
		}

		private static T Proxy<T>(Type type, object source, ProxyType proxyType)
		{
			// ReSharper disable once SuggestVarOrType_SimpleTypes (must box to support value types)
			object target = Activator.CreateInstance(TypeKitchen.Proxy.Create(type, proxyType));
			ShallowCopy(ref source, ref target);
			return (T) target;
		}

		private static void ShallowCopy(ref object source, ref object target)
		{
			var reader = ReadAccessor.Create(source, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
				AccessorMemberScope.Public, out var ra);
			var writer = WriteAccessor.Create(target, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties,
				AccessorMemberScope.Public, out var wa);

			foreach (var member in ra)
			{
				var hasValue = reader.TryGetValue(source, member.Name, out var value);
				var hasSetter = wa.TryGetValue(member.Name, out var wm);
				if (hasValue && hasSetter && wm.CanWrite)
					writer.TrySetValue(target, member.Name, value);
			}
		}

		private static T Marshal<T>(ref object instance)
		{
			// FIXME: Investigate converting T to a formatted class: 
			// See: https://social.msdn.microsoft.com/Forums/vstudio/en-US/0372911d-c200-47f0-91ac-a35428751e6b/what-is-a-formatted-class?forum=clr
			var handle = GCHandle.Alloc(instance, GCHandleType.Pinned);
			var allocated = (T) System.Runtime.InteropServices.Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return allocated;
		}
	}
}
