// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace TypeKitchen
{
	public static partial class LateBinding
	{
		#region CallSite

		public static Dictionary<string, Func<object, object>> CallSiteBindGet(AccessorMembers members)
		{
			return members.Members.Where(member => member.CanRead)
				.ToDictionary(member => member.Name, CallSiteBindGet);
		}

		public static Dictionary<string, Action<object, object>> CallSiteBindSet(AccessorMembers members)
		{
			return members.Members.Where(member => member.CanWrite)
				.ToDictionary(member => member.Name, CallSiteBindSet);
		}

		public static Func<object, object> CallSiteBindGet(AccessorMember member)
		{
			var args = new List<CSharpArgumentInfo> {CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)};
			var binder = Binder.GetMember(CSharpBinderFlags.None, member.Name, member.MemberInfo.DeclaringType, args);
			var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
			return t => callsite.Target(callsite, t);
		}

		public static Action<object, object> CallSiteBindSet(AccessorMember member)
		{
			var args = new List<CSharpArgumentInfo>
			{
				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
				CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
			};
			var binder = Binder.SetMember(CSharpBinderFlags.None, member.Name, member.MemberInfo.DeclaringType, args);
			var callsite = CallSite<Action<CallSite, object, object>>.Create(binder);
			return (t, v) => callsite.Target(callsite, t, v);
		}

		#endregion

		#region DynamicMethod

		public static Dictionary<string, Func<object, object>> DynamicMethodBindGet(AccessorMembers members)
		{
			return members.Members.Where(member => member.CanRead)
				.ToDictionary(member => member.Name, DynamicMethodBindGet);
		}

		public static Dictionary<string, Action<object, object>> DynamicMethodBindSet(AccessorMembers members)
		{
			return members.Members.Where(member => member.CanWrite)
				.ToDictionary(member => member.Name, DynamicMethodBindSet);
		}

		public static Func<object, object> DynamicMethodBindGet(AccessorMember member)
		{
			var skipVisibility = member.Type.IsNotPublic;
			var dm = new DynamicMethod($"{member.Name}", typeof(object), new[] {typeof(object)}, skipVisibility);
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
						il.Emit(OpCodes.Ldsfld, field);
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
			return (Func<object, object>) dm.CreateDelegate(typeof(Func<object, object>));
		}

		public static Action<object, object> DynamicMethodBindSet(AccessorMember member)
		{
			var skipVisibility = member.Type.IsNotPublic;
			var dm = new DynamicMethod($"{member.Name}", typeof(void), new[] {typeof(object), typeof(object)},
				skipVisibility);
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

			var declaringType = member.MemberInfo.DeclaringType;

			il.Emit(OpCodes.Ldarg_0); // target
			il.Emit(OpCodes.Castclass, declaringType); // (Type)target
			il.Emit(OpCodes.Ldarg_1); // value
			il.Emit(OpCodes.Castclass, memberType); // ({member.Type}) value

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
			return (Action<object, object>) dm.CreateDelegate(typeof(Action<object, object>));
		}

		#endregion

		#region Open Delegates

		public static Dictionary<string, Func<object, object>> OpenDelegateBindGet<TTarget>(AccessorMembers members)
			where TTarget : class
		{
			return members.Members.Where(member => member.CanRead)
				.ToDictionary(member => member.Name, OpenDelegateBindGet<TTarget>);
		}

		public static Dictionary<string, Action<object, object>> OpenDelegateBindSet<TTarget>(AccessorMembers members)
			where TTarget : class
		{
			return members.Members.Where(member => member.CanWrite)
				.ToDictionary(member => member.Name, OpenDelegateBindSet<TTarget>);
		}

		public static Func<object, object> OpenDelegateBindGet<TTarget>(AccessorMember member) where TTarget : class
		{
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

					return o => getter.Invoke((TTarget) o, null);
				}

				case FieldInfo field:
				{
					var getField =
						(Func<FieldInfo, TTarget, object>) Delegate.CreateDelegate(
							typeof(Func<FieldInfo, TTarget, object>), FieldGetValue);
					return t => getField.Invoke(field, (TTarget) t);
				}

				default:
					throw new ArgumentNullException();
			}
		}

		public static Action<object, object> OpenDelegateBindSet<TTarget>(AccessorMember member)
			where TTarget : class
		{
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

					return (t, v) => setter.Invoke((TTarget) t, v);
				}

				case FieldInfo field:
				{
					var setField =
						(Action<FieldInfo, TTarget, object>) Delegate.CreateDelegate(
							typeof(Action<FieldInfo, TTarget, object>), FieldSetValue);
					return (t, v) => setField.Invoke(field, (TTarget) t, v);
				}

				default:
					throw new ArgumentNullException();
			}
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
			return members.Members.Where(member => member.CanRead)
				.ToDictionary(member => member.Name, ExpressionBindGet);
		}

		public static Dictionary<string, Action<object, object>> ExpressionBindSet(AccessorMembers members)
		{
			return members.Members.Where(member => member.CanWrite)
				.ToDictionary(member => member.Name, ExpressionBindSet);
		}

		public static Func<object, object> ExpressionBindGet(AccessorMember member)
		{
			var declaringType = member.MemberInfo.DeclaringType;
			if (declaringType == null)
				throw new ArgumentNullException();

			switch (member.MemberInfo)
			{
				case PropertyInfo property:
				{
					var getMethod = property.GetGetMethod();
					if (getMethod == null)
						throw new ArgumentNullException();
					var targetParam = Expression.Parameter(typeof(object), "target");
					var call = Expression.Call(Expression.Convert(targetParam, declaringType), getMethod);
					var lambda =
						Expression.Lambda<Func<object, object>>(Expression.Convert(call, typeof(object)), targetParam);
					return lambda.Compile();
				}

				case FieldInfo field:
				{
					var targetParam = Expression.Parameter(typeof(object), "target");
					var getField = Expression.Field(Expression.Convert(targetParam, declaringType), field);
					var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(getField, typeof(object)),
						targetParam);
					return lambda.Compile();
				}

				default:
					throw new ArgumentException();
			}
		}

		public static Action<object, object> ExpressionBindSet(AccessorMember member)
		{
			var declaringType = member.MemberInfo.DeclaringType;
			if (declaringType == null)
				throw new ArgumentNullException();

			switch (member.MemberInfo)
			{
				case PropertyInfo property:
				{
					var setMethod = property.GetSetMethod();
					if (setMethod == null)
						throw new ArgumentNullException();

					var targetParam = Expression.Parameter(typeof(object), "target");
					var valueParam = Expression.Parameter(typeof(object), "value");
					var call = Expression.Call(Expression.Convert(targetParam, declaringType), setMethod,
						Expression.Convert(valueParam, property.PropertyType));
					var lambda = Expression.Lambda<Action<object, object>>(call, targetParam, valueParam);
					return lambda.Compile();
				}

				case FieldInfo field:
				{
					var targetParam = Expression.Parameter(typeof(object), "target");
					var valueParam = Expression.Parameter(typeof(object), "value");
					var setField = Expression.Field(Expression.Convert(targetParam, declaringType), field);
					var lambda = Expression.Lambda<Action<object, object>>(
						Expression.Convert(Expression.Assign(setField, Expression.Convert(valueParam, field.FieldType)),
							typeof(object)), targetParam, valueParam);
					return lambda.Compile();
				}

				default:
					throw new ArgumentException();
			}
		}

		#endregion

		#region Method Invoke

		public static Dictionary<string, Func<object, object>> MethodInvokeBindGet(AccessorMembers members)
		{
			return members.Members.Where(member => member.CanRead)
				.ToDictionary(member => member.Name, MethodInvokeBindGet);
		}

		public static Dictionary<string, Action<object, object>> MethodInvokeBindSet(AccessorMembers members)
		{
			return members.Members.Where(member => member.CanRead)
				.ToDictionary(member => member.Name, MethodInvokeBindSet);
		}

		public static Func<object, object> MethodInvokeBindGet(AccessorMember member)
		{
			switch (member.MemberInfo)
			{
				case PropertyInfo property:
				{
					var getMethod = property.GetGetMethod();
					if (getMethod == null)
						throw new ArgumentNullException();
					return o => getMethod.Invoke(o, null);
				}

				case FieldInfo field:
				{
					return o => field.GetValue(o);
				}

				default:
					throw new ArgumentException();
			}
		}

		public static Action<object, object> MethodInvokeBindSet(AccessorMember member)
		{
			switch (member.MemberInfo)
			{
				case PropertyInfo property:
				{
					var setMethod = property.GetSetMethod();
					if (setMethod == null)
						throw new ArgumentNullException();
					return (o, v) => setMethod.Invoke(o, new[] {v});
				}

				case FieldInfo field:
					return (o, v) => field.SetValue(o, v);
				default:
					throw new ArgumentException();
			}
		}

		#endregion
	}
}