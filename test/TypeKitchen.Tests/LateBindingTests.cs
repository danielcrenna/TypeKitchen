// Copyright (c) Blowdart, Inc. All rights reserved.

using System;
using System.Collections.Generic;
using TypeKitchen.Tests.Fakes;
using Xunit;

namespace TypeKitchen.Tests
{
    public class LateBindingTests
    {
        [Theory]
        [InlineData(LateBindingStrategy.CallSite)]
        [InlineData(LateBindingStrategy.DynamicMethod)]
        [InlineData(LateBindingStrategy.Expression)]
        [InlineData(LateBindingStrategy.MethodInvoke)]
        [InlineData(LateBindingStrategy.OpenDelegate)]
        public void GetTests(LateBindingStrategy strategy)
        {
            CanReadPropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
            CanReadPropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
            CanReadPropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
            CanReadPropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
            CanReadPropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
        }

        [Theory]
        [InlineData(LateBindingStrategy.CallSite)]
        [InlineData(LateBindingStrategy.DynamicMethod)]
        [InlineData(LateBindingStrategy.Expression)]
        [InlineData(LateBindingStrategy.MethodInvoke)]
        [InlineData(LateBindingStrategy.OpenDelegate)]
        public void SetTests(LateBindingStrategy strategy)
        {
            CanWritePropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
            CanWritePropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
            CanWritePropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
            CanWritePropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
            CanWritePropertyAndField(strategy, AccessorMemberScope.All, AccessorMemberTypes.All);
        }

        private static void CanReadPropertyAndField(LateBindingStrategy strategy, AccessorMemberScope scope,
            AccessorMemberTypes types)
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var map = BindGet(strategy, target, scope, types);

            Assert.Equal("Bar", map["Foo"](target));
            Assert.Equal("Baz", map["Bar"](target));
        }

        private static void CanWritePropertyAndField(LateBindingStrategy strategy, AccessorMemberScope scope,
            AccessorMemberTypes types)
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};

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
            var members = AccessorMembers.Create(target.GetType(), scope, memberTypes);
            Dictionary<string, Func<object, object>> map;
            switch (strategy)
            {
                case LateBindingStrategy.CallSite:
                    map = LateBinding.CallSiteBindingGet(members);
                    break;
                case LateBindingStrategy.DynamicMethod:
                    map = LateBinding.DynamicMethodBindingGet(members);
                    break;
                case LateBindingStrategy.Expression:
                    map = LateBinding.ExpressionBindGet(members);
                    break;
                case LateBindingStrategy.MethodInvoke:
                    map = LateBinding.MethodInvokeBindingGet(members);
                    break;
                case LateBindingStrategy.OpenDelegate:
                    map = LateBinding.OpenDelegateBindGet<OnePropertyOneField>(members);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }

            return map;
        }

        private static Dictionary<string, Action<object, object>> BindSet(LateBindingStrategy strategy, object target,
            AccessorMemberScope scope, AccessorMemberTypes memberTypes)
        {
            var members = AccessorMembers.Create(target.GetType(), scope, memberTypes);
            Dictionary<string, Action<object, object>> map;
            switch (strategy)
            {
                case LateBindingStrategy.CallSite:
                    map = LateBinding.CallSiteBindingSet(members);
                    break;
                case LateBindingStrategy.DynamicMethod:
                    map = LateBinding.DynamicMethodBindingSet(members);
                    break;
                case LateBindingStrategy.Expression:
                    map = LateBinding.ExpressionBindSet(members);
                    break;
                case LateBindingStrategy.MethodInvoke:
                    map = LateBinding.MethodInvokeBindingSet(members);
                    break;
                case LateBindingStrategy.OpenDelegate:
                    map = LateBinding.OpenDelegateBindSet<OnePropertyOneField>(members);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }

            return map;
        }
    }
}
