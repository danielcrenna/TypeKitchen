// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace TypeKitchen
{
	public interface ITypeResolver
	{
		Type FindByFullName(string fullName);
		Type FindFirstByName(string name);
		Type FindFirstByMethodName(string methodName);
		IEnumerable<Type> FindByMethodName(string methodName);
		IEnumerable<Type> FindByInterface<TInterface>();
		IEnumerable<Type> FindByParent<T>();
	}
}