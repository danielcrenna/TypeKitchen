namespace TypeKitchen
{
	public static class CallAccessorExtensions
	{
		public static object Call(this IMethodCallAccessor accessor, object target)
		{
			var args = Pooling.Arguments.Get(0);
			try
			{
				return accessor.Call(target, args);
			}
			finally
			{
				Pooling.Arguments.Return(args);
			}
		}

		public static object Call(this IMethodCallAccessor accessor, object target, object arg1)
		{
			var args = Pooling.Arguments.Get(1);
			args[0] = arg1;
			try
			{
				return accessor.Call(target, args);
			}
			finally
			{
				Pooling.Arguments.Return(args);
			}
		}

		public static object Call(this IMethodCallAccessor accessor, object target, object arg1, object arg2)
		{
			var args = Pooling.Arguments.Get(2);
			args[0] = arg1;
			args[1] = arg2;
			try
			{
				return accessor.Call(target, args);
			}
			finally
			{
				Pooling.Arguments.Return(args);
			}
		}

		public static object Call(this IMethodCallAccessor accessor, object target, object arg1, object arg2, object arg3)
		{
			var args = Pooling.Arguments.Get(3);
			args[0] = arg1;
			args[1] = arg2;
			args[2] = arg3;
			try
			{
				return accessor.Call(target, args);
			}
			finally
			{
				Pooling.Arguments.Return(args);
			}
		}
	}
}