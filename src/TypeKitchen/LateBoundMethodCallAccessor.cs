// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace TypeKitchen
{
	internal sealed class LateBoundMethodCallAccessor : MethodCallAccessor
	{
		private readonly Func<object, object[], object> _binding;

		public LateBoundMethodCallAccessor(MethodInfo method)
		{
			_binding = LateBinding.DynamicMethodBindCall(method);

			MethodName = method.Name;
			Parameters = method.GetParameters();
		}

		public override object Call(object target, object[] args)
		{
			return _binding(target, args);
		}
	}
}