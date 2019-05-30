// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace TypeKitchen.Tests.Fakes
{
	public sealed class DirectCallAccessor : ITypeCallAccessor, IMethodCallAccessor
	{
		public MethodInfo MethodInfo =>
			typeof(ClassWithTwoMethodsAndProperty).GetMethod(nameof(ClassWithTwoMethodsAndProperty.Foo));

		public string MethodName => MethodInfo.Name;
		public ParameterInfo[] Parameters => MethodInfo.GetParameters();

		public object Call(object target)
		{
			((ClassWithTwoMethodsAndProperty) target).Foo();
			return null;
		}

		public object Call(object target, object arg)
		{
			((ClassWithTwoMethodsAndProperty) target).Foo();
			return null;
		}

		public object Call(object target, object arg1, object arg2)
		{
			((ClassWithTwoMethodsAndProperty) target).Foo();
			return null;
		}

		public object Call(object target, object arg1, object arg2, object arg3)
		{
			((ClassWithTwoMethodsAndProperty) target).Foo();
			return null;
		}

		public object Call(object target, object[] args)
		{
			((ClassWithTwoMethodsAndProperty) target).Foo();
			return null;
		}

		public Type Type => typeof(ClassWithTwoMethodsAndProperty);

		public object Call(object target, string key, params object[] args)
		{
			switch (key)
			{
				case "Foo":
					((ClassWithTwoMethodsAndProperty) target).Foo();
					return typeof(void);
				case "Bar":
					((ClassWithTwoMethodsAndProperty) target).Bar((int) args[0]);
					return typeof(void);
				case "Method":
					return typeof(ClassWithTwoMethodsAndProperty).GetMethod("Method").Invoke(null, null);
				default:
					throw new ArgumentNullException();
			}
		}
	}
}