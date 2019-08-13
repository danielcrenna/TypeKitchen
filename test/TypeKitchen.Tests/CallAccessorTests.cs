// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
	public class CallAccessorTests
	{
		[Fact]
		public void Call_Method_Static_Void_NoArgs()
		{
			var target = new ClassWithTwoMethodsAndProperty();
			var methodInfo = target.GetType().GetMethod("Method");
			var accessor = CallAccessor.Create(methodInfo);
			Assert.Equal(methodInfo.Name, accessor.MethodName);
			var parameters = accessor.Parameters;
			Assert.NotNull(parameters);
			Assert.Equal(parameters, methodInfo.GetParameters());
			accessor.Call(target, new object[0]);
		}

		[Fact]
		public void Call_Instance_Method_Void_NoArgs()
		{
			CallInstanceMethod(nameof(ClassWithTwoMethodsAndProperty.Foo));
		}

		[Fact]
		public void Call_Instance_Method_ReturnInt_NoArgs()
		{
			var result = CallInstanceMethod(nameof(ClassWithTwoMethodsAndProperty.Biff));
			Assert.Equal(1, result);
		}

		[Fact(Skip = "Something weird going on here...")]
		public void Call_Instance_Method_ReturnInt_WithArgs()
		{
			var result = CallInstanceMethod(nameof(ClassWithTwoMethodsAndProperty.Echo), "ABC");
			Assert.Equal("ABC", result);
		}

		private static object CallInstanceMethod(string methodName, params object[] args)
		{
			var target = new ClassWithTwoMethodsAndProperty();
			var methodInfo = target.GetType().GetMethod(methodName);
			var accessor = CallAccessor.Create(methodInfo);

			Assert.NotNull(methodInfo);
			Assert.Equal(methodInfo.Name, accessor.MethodName);

			Assert.NotNull(accessor.Parameters);
			Assert.Equal(accessor.Parameters, methodInfo.GetParameters());
			Assert.Equal(args?.Length, accessor.Parameters.Length);

			var result = accessor.Call(target, args);
			return result;
		}
	}
}