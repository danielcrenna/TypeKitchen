// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace TypeKitchen
{
	public abstract class MethodCallAccessor : IMethodCallAccessor
	{
		public string MethodName { get; set; }
		public ParameterInfo[] Parameters { get; set; }
		public abstract object Call(object target, object[] args);
	}
}