// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TypeKitchen.Serialization
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
			var data = Wire.Simple(instance);
			var type = instance.GetType();
			
			using var buffer = new MemoryStream(data);
			using var br = new BinaryReader(buffer);
			var copy = br.ReadObject(type);
			
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
				instance.Add(ShallowCopy(item));
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
				instance.Add(key, ShallowCopy(value));
			}

			return instance;
		}
	}
}