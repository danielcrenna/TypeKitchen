// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace TypeKitchen
{
	public abstract class MethodCallAccessor : IMethodCallAccessor
	{
		public string MethodName { get; set; }
		public ParameterInfo[] Parameters { get; set; }

		public object Call(object target)
		{
			var args = Pooling.Arguments.Get(0);
			try
			{
				return Call(target, args);
			}
			finally
			{
				Pooling.Arguments.Return(args);
			}
		}

		public object Call(object target, object arg1)
		{
			var args = Pooling.Arguments.Get(1);
			args[0] = arg1;
			try
			{
				return Call(target, args);
			}
			finally
			{
				Pooling.Arguments.Return(args);
			}
		}

		public object Call(object target, object arg1, object arg2)
		{
			var args = Pooling.Arguments.Get(2);
			args[0] = arg1;
			args[1] = arg2;
			try
			{
				return Call(target, args);
			}
			finally
			{
				Pooling.Arguments.Return(args);
			}
		}

		public object Call(object target, object arg1, object arg2, object arg3)
		{
			var args = Pooling.Arguments.Get(3);
			args[0] = arg1;
			args[1] = arg2;
			args[2] = arg3;
			try
			{
				return Call(target, args);
			}
			finally
			{
				Pooling.Arguments.Return(args);
			}
		}

		public abstract object Call(object target, object[] args);
	}
}