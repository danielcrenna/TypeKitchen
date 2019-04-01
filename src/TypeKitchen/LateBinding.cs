// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace TypeKitchen
{
    public static class LateBinding
    {
        #region CallSite 

        public static Dictionary<string, Func<object, object>> CallSiteBindingGet(AccessorMembers members)
        {
            var map = new Dictionary<string, Func<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;
                var args = new List<CSharpArgumentInfo> {CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)};
                var binder = Binder.GetMember(CSharpBinderFlags.None, member.Name, members.DeclaringType, args);
                var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
                map.Add(member.Name, t => callsite.Target(callsite, t));
            }

            return map;
        }

        public static Dictionary<string, Action<object, object>> CallSiteBindingSet(AccessorMembers members)
        {
            var map = new Dictionary<string, Action<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;
                var args = new List<CSharpArgumentInfo>
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                };
                var binder = Binder.SetMember(CSharpBinderFlags.None, member.Name, members.DeclaringType, args);
                var callsite = CallSite<Action<CallSite, object, object>>.Create(binder);
                map.Add(member.Name, (t, v) => callsite.Target(callsite, t, v));
            }

            return map;
        }

        #endregion

        #region DynamicMethod

        public static Dictionary<string, Func<object, object>> DynamicMethodBindingGet(AccessorMembers members)
        {
            var map = new Dictionary<string, Func<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;
                var name = member.Name;
                var dm = new DynamicMethod($"{name}", typeof(object), new[] {typeof(object)});
                var il = dm.GetILGenerator();
                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                    {
                        var getMethod = property.GetGetMethod();
                        if (getMethod == null)
                            throw new ArgumentNullException();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(getMethod.IsFinal || !getMethod.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, getMethod);
                        if (property.PropertyType.IsValueType)
                            il.Emit(OpCodes.Box, property.PropertyType);
                        break;
                    }
                    case FieldInfo field:
                        if (field.IsStatic)
                        {
                            il.Emit(OpCodes.Ldsfld, field);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, field);
                        }

                        if (field.FieldType.IsValueType)
                            il.Emit(OpCodes.Box, field.FieldType);
                        break;
                }

                il.Emit(OpCodes.Ret);
                var @delegate = (Func<object, object>) dm.CreateDelegate(typeof(Func<object, object>));
                map.Add(name, @delegate);
            }

            return map;
        }

        public static Dictionary<string, Action<object, object>> DynamicMethodBindingSet(AccessorMembers members)
        {
            var map = new Dictionary<string, Action<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;
                var name = member.Name;
                var dm = new DynamicMethod($"{name}", typeof(void), new[] {typeof(object), typeof(object)});
                var il = dm.GetILGenerator();

                Type memberType;
                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                        memberType = property.PropertyType;
                        break;
                    case FieldInfo field:
                        memberType = field.FieldType;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(member));
                }

                il.Emit(OpCodes.Ldarg_0);                               // target
                il.Emit(OpCodes.Castclass, members.DeclaringType);      // (Type)target
                il.Emit(OpCodes.Ldarg_1);                               // value
                il.Emit(OpCodes.Castclass, memberType);                 // ({member.Type}) value

                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                        var setMethod = property.GetSetMethod();
                        il.Emit(setMethod.IsFinal || !setMethod.IsVirtual ? OpCodes.Call : OpCodes.Callvirt, setMethod);
                        break;
                    case FieldInfo field:
                        il.Emit(OpCodes.Stfld, field);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(member));
                }

                il.Emit(OpCodes.Ret);
                var @delegate = (Action<object, object>) dm.CreateDelegate(typeof(Action<object, object>));
                map.Add(name, @delegate);
            }

            return map;
        }

        #endregion

        #region Open Delegates

        public static Dictionary<string, Func<object, object>> OpenDelegateBindGet<TTarget>(AccessorMembers members)
            where TTarget : class
        {
            var map = new Dictionary<string, Func<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;

                if (member.Type.IsValueType)
                    throw new NotImplementedException();

                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                    {
                        var getProperty = property.GetGetMethod();
                        Func<TTarget, object, object> getter;
                        var type = typeof(TTarget);
                        var parameters = getProperty.GetParameters();
                        switch (parameters.Length)
                        {
                            case 0:
                            {
                                var func = Delegate.CreateDelegate(
                                    typeof(Func<,>).MakeGenericType(type, getProperty.ReturnType), getProperty);
                                getter = (Func<TTarget, object, object>) CallGetWithNoArguments
                                    .MakeGenericMethod(type, getProperty.ReturnType).Invoke(null, new object[] {func});
                                break;
                            }
                            case 1:
                            {
                                var func = Delegate.CreateDelegate(
                                    typeof(Func<,,>).MakeGenericType(type, parameters[0].ParameterType,
                                        getProperty.ReturnType), getProperty);
                                getter = (Func<TTarget, object, object>) CallGetWithOneArgument
                                    .MakeGenericMethod(type, parameters[0].ParameterType, getProperty.ReturnType)
                                    .Invoke(null, new object[] {func});
                                break;
                            }
                            default:
                                throw new NotImplementedException();
                        }

                        map.Add(member.Name, o => getter.Invoke((TTarget) o, null));
                        continue;
                    }
                    case FieldInfo field:
                    {
                        var getField =
                            (Func<FieldInfo, TTarget, object>) Delegate.CreateDelegate(
                                typeof(Func<FieldInfo, TTarget, object>), FieldGetValue);
                        map.Add(member.Name, t => getField.Invoke(field, (TTarget) t));
                        continue;
                    }
                }
            }

            return map;
        }

        public static Dictionary<string, Action<object, object>> OpenDelegateBindSet<TTarget>(AccessorMembers members)
            where TTarget : class
        {
            var map = new Dictionary<string, Action<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;

                if (member.Type.IsValueType)
                    throw new NotImplementedException();

                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                    {
                        var setProperty = property.GetSetMethod();
                        Action<TTarget, object> setter;
                        var type = typeof(TTarget);
                        var parameters = setProperty.GetParameters();
                        switch (parameters.Length)
                        {
                            case 0:
                            {
                                var action = Delegate.CreateDelegate(
                                    typeof(Action<,>).MakeGenericType(type, parameters[0].ParameterType), setProperty);
                                setter = (Action<TTarget, object>) CallSetWithValue
                                    .MakeGenericMethod(type, parameters[0].ParameterType)
                                    .Invoke(null, new object[] {action});
                                break;
                            }
                            case 1:
                            {
                                var action = Delegate.CreateDelegate(
                                    typeof(Action<,>).MakeGenericType(type, parameters[0].ParameterType), setProperty);
                                setter = (Action<TTarget, object>) CallSetWithValue
                                    .MakeGenericMethod(type, parameters[0].ParameterType)
                                    .Invoke(null, new object[] {action});
                                break;
                            }
                            default:
                                throw new NotImplementedException();
                        }

                        map.Add(member.Name, (t, v) => setter.Invoke((TTarget) t, v));
                        continue;
                    }
                    case FieldInfo field:
                    {
                        var setField =
                            (Action<FieldInfo, TTarget, object>) Delegate.CreateDelegate(
                                typeof(Action<FieldInfo, TTarget, object>), FieldSetValue);
                        map.Add(member.Name, (t, v) => setField.Invoke(field, (TTarget) t, v));
                        continue;
                    }
                }
            }

            return map;
        }

        #region Open Delegate Helpers

        private static readonly MethodInfo FieldGetValue = typeof(FieldInfo).GetMethod(nameof(FieldInfo.GetValue));

        private static readonly MethodInfo CallGetWithNoArguments =
            typeof(LateBinding).GetMethod(nameof(GetWithNoArguments), BindingFlags.NonPublic | BindingFlags.Static);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<TTarget, object, object> GetWithNoArguments<TTarget, TReturn>(Func<TTarget, TReturn> func)
            where TTarget : class
        {
            return (target, param) => func(target);
        }

        private static readonly MethodInfo CallGetWithOneArgument =
            typeof(LateBinding).GetMethod(nameof(GetWithOneArgument), BindingFlags.NonPublic | BindingFlags.Static);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<TTarget, object, object> GetWithOneArgument<TTarget, TParameter, TReturn>(
            Func<TTarget, TParameter, TReturn> func) where TTarget : class
        {
            return (target, param) => func(target, (TParameter) param);
        }

        private static readonly MethodInfo FieldSetValue =
            typeof(FieldInfo).GetMethod(nameof(FieldInfo.SetValue), new[] {typeof(object), typeof(object)});

        private static readonly MethodInfo CallSetWithValue =
            typeof(LateBinding).GetMethod(nameof(SetWithValue), BindingFlags.NonPublic | BindingFlags.Static);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Action<TTarget, object> SetWithValue<TTarget, TValue>(Action<TTarget, TValue> action)
            where TTarget : class
        {
            return (target, value) => action(target, (TValue) value);
        }

        #endregion

        #endregion

        #region Expression

        public static Dictionary<string, Func<object, object>> ExpressionBindGet(AccessorMembers members)
        {
            var map = new Dictionary<string, Func<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;

                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                    {
                        var getMethod = property.GetGetMethod();
                        if (getMethod == null)
                            throw new ArgumentNullException();

                        var declaringType = members.DeclaringType;
                        if (declaringType == null)
                            throw new ArgumentNullException();

                        var targetParam = Expression.Parameter(typeof(object), "target");
                        var call = Expression.Call(Expression.Convert(targetParam, declaringType), getMethod);
                        var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(call, typeof(object)),
                            targetParam);
                        map.Add(member.Name, lambda.Compile());
                        break;
                    }
                    case FieldInfo field:
                    {
                        var declaringType = members.DeclaringType;
                        if (declaringType == null)
                            throw new ArgumentNullException();

                        var targetParam = Expression.Parameter(typeof(object), "target");
                        var getField = Expression.Field(Expression.Convert(targetParam, declaringType), field);
                        var lambda =
                            Expression.Lambda<Func<object, object>>(Expression.Convert(getField, typeof(object)),
                                targetParam);
                        map.Add(member.Name, lambda.Compile());
                        break;
                    }
                }
            }

            return map;
        }

        public static Dictionary<string, Action<object, object>> ExpressionBindSet(AccessorMembers members)
        {
            var map = new Dictionary<string, Action<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;

                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                    {
                        var setMethod = property.GetSetMethod();
                        if (setMethod == null)
                            throw new ArgumentNullException();

                        var declaringType = members.DeclaringType;
                        if (declaringType == null)
                            throw new ArgumentNullException();

                        var targetParam = Expression.Parameter(typeof(object), "target");
                        var valueParam = Expression.Parameter(typeof(object), "value");
                        var call = Expression.Call(Expression.Convert(targetParam, declaringType), setMethod,
                            Expression.Convert(valueParam, property.PropertyType));
                        var lambda = Expression.Lambda<Action<object, object>>(call, targetParam, valueParam);
                        map.Add(member.Name, lambda.Compile());
                        break;
                    }
                    case FieldInfo field:
                    {
                        var declaringType = members.DeclaringType;
                        if (declaringType == null)
                            throw new ArgumentNullException();

                        var targetParam = Expression.Parameter(typeof(object), "target");
                        var valueParam = Expression.Parameter(typeof(object), "value");
                        var setField = Expression.Field(Expression.Convert(targetParam, declaringType), field);
                        var lambda = Expression.Lambda<Action<object, object>>(
                            Expression.Convert(
                                Expression.Assign(setField, Expression.Convert(valueParam, field.FieldType)),
                                typeof(object)), targetParam, valueParam);
                        map.Add(member.Name, lambda.Compile());
                        break;
                    }
                }
            }

            return map;
        }

        #endregion

        #region Method Invoke

        public static Dictionary<string, Func<object, object>> MethodInvokeBindingGet(AccessorMembers members)
        {
            var map = new Dictionary<string, Func<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;
                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                    {
                        var getMethod = property.GetGetMethod();
                        if (getMethod == null)
                            throw new ArgumentNullException();
                        map.Add(member.Name, o => getMethod.Invoke(o, null));
                        break;
                    }
                    case FieldInfo field:
                        map.Add(member.Name, o => field.GetValue(o));
                        break;
                }
            }

            return map;
        }

        public static Dictionary<string, Action<object, object>> MethodInvokeBindingSet(AccessorMembers members)
        {
            var map = new Dictionary<string, Action<object, object>>();
            foreach (var member in members.Members)
            {
                if (!member.CanRead)
                    continue;
                switch (member.MemberInfo)
                {
                    case PropertyInfo property:
                    {
                        var setMethod = property.GetSetMethod();
                        if (setMethod == null)
                            throw new ArgumentNullException();
                        map.Add(member.Name, (o, v) => setMethod.Invoke(o, new[] {v}));
                        break;
                    }
                    case FieldInfo field:
                        map.Add(member.Name, (o, v) => field.SetValue(o, v));
                        break;
                }
            }

            return map;
        }

        #endregion
    }
}