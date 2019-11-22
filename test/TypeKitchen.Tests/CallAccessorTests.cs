// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
	public class CallAccessorTests
	{
		[Fact]
		public void Call_Static_Method_Returns_NoArgs()
		{
			var target = new ClassWithTwoMethodsAndProperty();
			
			var methodInfo = target.GetType().GetMethod("Method");
			var accessor = CallAccessor.Create(methodInfo);
			Assert.Equal(methodInfo.Name, accessor.MethodName);
			
			var parameters = accessor.Parameters;
			Assert.NotNull(parameters);
			Assert.Equal(parameters, methodInfo.GetParameters());

			var result = accessor.Call(target, new object[0]);
			Assert.Equal(1, result);
		}

		[Fact]
		public void Call_Static_Method_Returns_Args()
		{
			var methodInfo = typeof(ClassWithTwoMethodsAndProperty)
				.GetMethod(nameof(ClassWithTwoMethodsAndProperty.StaticEcho));

			var accessor = CallAccessor.Create(methodInfo);
			Assert.Equal(methodInfo.Name, accessor.MethodName);
			
			var parameters = accessor.Parameters;
			Assert.NotNull(parameters);
			Assert.Equal(parameters, methodInfo.GetParameters());

			var result = accessor.Call(null, "ABC");
			Assert.Equal("ABC", result);
		}

		[Fact]
		public void Call_Instance_Method_Void_NoArgs()
		{
			CallInstanceMethod(nameof(ClassWithTwoMethodsAndProperty.Foo));
		}

		[Fact]
		public void Call_Instance_Method_Returns_NoArgs()
		{
			var result = CallInstanceMethod(nameof(ClassWithTwoMethodsAndProperty.Biff));
			Assert.Equal(1, result);
		}

		[Fact]
		public void Call_Instance_Method_Returns_WithArgs()
		{
			var result = CallInstanceMethod(nameof(ClassWithTwoMethodsAndProperty.Echo), "ABC");
			Assert.Equal("ABC", result);
		}

		private static object CallInstanceMethod(string methodName)
		{
			var target = AssertCreationAspects(methodName, out var accessor);
			var result = accessor.Call(target);
			return result;
		}

		private static object CallInstanceMethod(string methodName, params object[] args)
		{
			var target = AssertCreationAspects(methodName, out var accessor);
			var result = accessor.Call(target, args);
			return result;
		}

		private static ClassWithTwoMethodsAndProperty AssertCreationAspects(string methodName, out IMethodCallAccessor accessor)
		{
			var target = new ClassWithTwoMethodsAndProperty();
			var methodInfo = target.GetType().GetMethod(methodName);
			accessor = CallAccessor.Create(methodInfo);

			Assert.NotNull(methodInfo);
			Assert.Equal(methodInfo.Name, accessor.MethodName);

			Assert.NotNull(accessor.Parameters);
			Assert.Equal(accessor.Parameters, methodInfo.GetParameters());
			return target;
		}
	}
}