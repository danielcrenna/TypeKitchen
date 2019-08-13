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
			accessor.Call(target);
		}

		[Fact]
		public void Call_Instance_Method_Void_NoArgs()
		{
			CallInstanceMethod(nameof(ClassWithTwoMethodsAndProperty.Foo));
		}


		[Fact]
		public void Call_Instance_Method_ReturnInt_NoArgs()
		{
			CallInstanceMethod(nameof(ClassWithTwoMethodsAndProperty.Biff));
		}

		[Fact]
		public void Call_Instance_Void_NoArgs()
		{
			var target = new ClassWithTwoMethodsAndProperty();
			var type = target.GetType();
			var accessor = CallAccessor.Create(type);
			Assert.Equal(type, accessor.Type);
			accessor.Call(target, "Foo");
		}


		private static void CallInstanceMethod(string methodName)
		{
			var target = new ClassWithTwoMethodsAndProperty();
			var methodInfo = target.GetType().GetMethod(methodName);
			var accessor = CallAccessor.Create(methodInfo);
			Assert.Equal(methodInfo.Name, accessor.MethodName);

			var parameters = accessor.Parameters;
			Assert.NotNull(parameters);
			Assert.Equal(parameters, methodInfo.GetParameters());
			accessor.Call(target);
		}
	}
}