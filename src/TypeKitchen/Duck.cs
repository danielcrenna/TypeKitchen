using System;

namespace TypeKitchen
{
	public static class Duck
	{
		public static T QuackLike<T>(this object source)
		{
			// ReSharper disable once SuggestVarOrType_SimpleTypes (must box to support value types)
			object target = Activator.CreateInstance(Proxy.Create(typeof(T), ProxyType.Mimic));

			var reader = ReadAccessor.Create(source, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var ra);
			var writer = WriteAccessor.Create(target, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var wa);

			foreach (var member in ra)
			{
				var hasValue = reader.TryGetValue(source, member.Name, out var value);
				var hasSetter = wa.TryGetValue(member.Name, out var wm);

				if (hasValue && hasSetter && wm.CanWrite)
					writer.TrySetValue(target, member.Name, value);
			}

			return (T) target;
		}
	}
}
