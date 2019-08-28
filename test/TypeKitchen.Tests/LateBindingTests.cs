// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
	public class LateBindingTests
	{
		[Theory]
		[InlineData(LateBindingStrategy.DynamicMethod)]
		public void CallTests(LateBindingStrategy strategy)
		{
			CanCallInstanceMethod(strategy, AccessorMemberScope.All, AccessorMemberTypes.Methods);
			CanCallStaticMethod(strategy, AccessorMemberScope.All, AccessorMemberTypes.Methods);
		}

		private static void CanCallInstanceMethod(LateBindingStrategy strategy, AccessorMemberScope scope,
			AccessorMemberTypes types)
		{
			var target = new ClassWithTwoMethodsAndProperty();
			var map = BindCall(strategy, target, scope, types);

			map["Foo"](target, new object[] { });
			map["Bar"](target, new object[] {100});
		}

		private static void CanCallStaticMethod(LateBindingStrategy strategy, AccessorMemberScope scope,
			AccessorMemberTypes types)
		{
			var target = new ClassWithTwoMethodsAndProperty();
			var map = BindCall(strategy, target, scope, types);

			map["Method"](null, new object[] { });
			map["Method"](null, null);
		}

		[Theory]
		[InlineData(LateBindingStrategy.CallSite)]
		[InlineData(LateBindingStrategy.DynamicMethod)]
		[InlineData(LateBindingStrategy.Expression)]
		[InlineData(LateBindingStrategy.MethodInvoke)]
		[InlineData(LateBindingStrategy.OpenDelegate)]
		public void GetTests(LateBindingStrategy strategy)
		{
			CanReadPropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
			CanReadPropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
			CanReadPropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
			CanReadPropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
			CanReadPropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
		}

		[Theory]
		[InlineData(LateBindingStrategy.CallSite)]
		[InlineData(LateBindingStrategy.DynamicMethod)]
		[InlineData(LateBindingStrategy.Expression)]
		[InlineData(LateBindingStrategy.MethodInvoke)]
		[InlineData(LateBindingStrategy.OpenDelegate)]
		public void SetTests(LateBindingStrategy strategy)
		{
			CanWritePropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
			CanWritePropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
			CanWritePropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
			CanWritePropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
			CanWritePropertyAndField(strategy, AccessorMemberScope.All,
				AccessorMemberTypes.Fields | AccessorMemberTypes.Properties);
		}

		private static void CanReadPropertyAndField(LateBindingStrategy strategy, AccessorMemberScope scope,
			AccessorMemberTypes types)
		{
			var target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};
			var map = BindGet(strategy, target, scope, types);

			Assert.Equal("Bar", map["Foo"](target));
			Assert.Equal("Baz", map["Bar"](target));
		}

		private static void CanWritePropertyAndField(LateBindingStrategy strategy, AccessorMemberScope scope,
			AccessorMemberTypes types)
		{
			var target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};

			var getMap = BindGet(strategy, target, scope, types);
			var setMap = BindSet(strategy, target, scope, types);

			setMap["Foo"](target, "Fizz");
			setMap["Bar"](target, "Buzz");

			Assert.Equal("Fizz", getMap["Foo"](target));
			Assert.Equal("Buzz", getMap["Bar"](target));
		}

		private static Dictionary<string, Func<object, object>> BindGet(LateBindingStrategy strategy, object target,
			AccessorMemberScope scope, AccessorMemberTypes memberTypes)
		{
			var members = AccessorMembers.Create(target.GetType(), memberTypes, scope);
			Dictionary<string, Func<object, object>> map;
			switch (strategy)
			{
				case LateBindingStrategy.CallSite:
					map = LateBinding.CallSiteBindGet(members);
					break;
				case LateBindingStrategy.DynamicMethod:
					map = LateBinding.DynamicMethodBindGet(members);
					break;
				case LateBindingStrategy.Expression:
					map = LateBinding.ExpressionBindGet(members);
					break;
				case LateBindingStrategy.MethodInvoke:
					map = LateBinding.MethodInvokeBindGet(members);
					break;
				case LateBindingStrategy.OpenDelegate:
					map = LateBinding.OpenDelegateBindGet<OnePropertyOneFieldStrings>(members);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
			}

			return map;
		}

		private static Dictionary<string, Action<object, object>> BindSet(LateBindingStrategy strategy, object target,
			AccessorMemberScope scope, AccessorMemberTypes memberTypes)
		{
			var members = AccessorMembers.Create(target.GetType(), memberTypes, scope);
			Dictionary<string, Action<object, object>> map;
			switch (strategy)
			{
				case LateBindingStrategy.CallSite:
					map = LateBinding.CallSiteBindSet(members);
					break;
				case LateBindingStrategy.DynamicMethod:
					map = LateBinding.DynamicMethodBindSet(members);
					break;
				case LateBindingStrategy.Expression:
					map = LateBinding.ExpressionBindSet(members);
					break;
				case LateBindingStrategy.MethodInvoke:
					map = LateBinding.MethodInvokeBindSet(members);
					break;
				case LateBindingStrategy.OpenDelegate:
					map = LateBinding.OpenDelegateBindSet<OnePropertyOneFieldStrings>(members);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
			}

			return map;
		}

		private static Dictionary<string, Func<object, object[], object>> BindCall(LateBindingStrategy strategy,
			object target, AccessorMemberScope scope, AccessorMemberTypes memberTypes)
		{
			var members = AccessorMembers.Create(target.GetType(), memberTypes, scope);
			Dictionary<string, Func<object, object[], object>> map;
			switch (strategy)
			{
				//case LateBindingStrategy.CallSite:
				//    map = LateBinding.CallSiteBindGet(members);
				//    break;
				case LateBindingStrategy.DynamicMethod:
					map = LateBinding.DynamicMethodBindCall(members);
					break;
				//case LateBindingStrategy.Expression:
				//    map = LateBinding.ExpressionBindGet(members);
				//    break;
				//case LateBindingStrategy.MethodInvoke:
				//    map = LateBinding.MethodInvokeBindGet(members);
				//    break;
				//case LateBindingStrategy.OpenDelegate:
				//    map = LateBinding.OpenDelegateBindGet<OnePropertyOneFieldStrings>(members);
				//    break;
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
			}

			return map;
		}
	}
}