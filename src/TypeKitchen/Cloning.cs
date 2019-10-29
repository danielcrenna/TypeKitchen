// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TypeKitchen.Internal;

namespace TypeKitchen
{
	public static class Cloning
	{
		public static object ShallowCopy(object instance)
		{
			var type = instance?.GetType();
			if (type == null)
				return null;

			if (type.IsValueType || type == typeof(string))
				return instance;

			if (instance is IEnumerable enumerable)
			{
				switch (instance)
				{
					case IList _:
						return ShallowCopyList(type, enumerable);
					case IDictionary _:
						return ShallowCopyDictionary(type, enumerable);
				}
			}
			
			return ShallowCopyObject(instance);
		}

		private static object ShallowCopyObject(object instance)
		{
			var copy = Instancing.CreateInstance(instance);
			var data = Wire.Simple(instance);

			var type = instance.GetType();
			var members = Wire.GetMembers(type);
			var write = Wire.GetPropertyWriter(type);
			
			using var buffer = new MemoryStream(data);
			using var br = new BinaryReader(buffer);
			foreach (var member in members)
			{
                if (!member.CanWrite)
	                continue;

                // FIXME: should not reach into Wire
				var value = Wire.ReadValue(member.Type, br);
				write.TrySetValue(copy, member.Name, value);
			}

			return copy;
		}

		private static object ShallowCopyList(Type type, IEnumerable enumerable)
		{
			var listType = typeof(IList<>).IsAssignableFromGeneric(type)
				? typeof(List<>).MakeGenericType(type.GenericTypeArguments)
				: type.GetElementType();

			var instance = (IList) Instancing.CreateInstance(listType);
			foreach (var item in enumerable)
			{
				var copy = ShallowCopy(item);
				instance.Add(copy);
			}

			return instance;
		}

		private static object ShallowCopyDictionary(Type type, IEnumerable dictionary)
		{
			var instance = (IDictionary) Instancing.CreateInstance(type);
			var pair = typeof(KeyValuePair<,>).MakeGenericType(type.GenericTypeArguments);

			foreach (var item in dictionary)
			{
				var key = pair.GetProperty("Key")?.GetValue(item);
				if (key == null)
					continue;

				var value = pair.GetProperty("Value")?.GetValue(item);
				var copy = ShallowCopy(value);
				instance.Add(key, copy);
			}

			return instance;
		}
	}
}