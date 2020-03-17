// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TypeKitchen.Reflection;

namespace TypeKitchen
{
	partial class LateBinding
	{
		#region DynamicMethod

		public static Dictionary<string, Func<object, object[], object>> DynamicMethodBindCall(AccessorMembers members)
		{
			return members.Members.Where(member => member.CanCall && member.IsInstanceMethod)
				.ToDictionary(member => member.Name, DynamicMethodBindCall);
		}

		public static Func<object, object[], object> DynamicMethodBindCall(AccessorMember member)
		{
			return DynamicMethodBindCall((MethodInfo) member.MemberInfo);
		}

		public static Func<object, object[], object> DynamicMethodBindCall(MethodInfo method)
		{
			var type = method.DeclaringType;
			if (type == null)
				throw new NotSupportedException("Dynamic binding does not currently support anonymous methods");

			var restrictedSkipVisibility = type.IsNotPublic;
			var dm = new DynamicMethod($"Call_{method.MetadataToken}", typeof(object),
				new[] {typeof(object), typeof(object[])}, restrictedSkipVisibility);
			dm.GetILGeneratorInternal().EmitCallDelegate(type, method);
			return (Func<object, object[], object>) dm.CreateDelegate(typeof(Func<object, object[], object>));
		}

		internal static void EmitCallDelegate(this ILSugar il, Type type, MethodInfo method)
		{
			if (!method.IsStatic)
			{
				// this
				var @this = il.DeclareLocal(type);
				il.Ldarg_0();
				il.Unbox_Any(type);
				il.Stloc(@this);
				if (type.IsValueType)
					il.Ldloca(@this);
				else
					il.Ldloc(@this);
			}

			var parameters = method.GetParameters();
			for (var i = 0; i < parameters.Length; i++)
			{
				il.Ldarg_1(); // args
				il.LoadConstant(i); // i
				il.Ldelem_Ref(); // args[i]

				var parameterType = parameters[i].ParameterType;
				var byRef = parameterType.IsByRef;
				if (byRef)
					parameterType = parameterType.GetElementType();

				var arg = il.DeclareLocal(parameterType);
				il.Unbox_Any(parameterType);
				il.Stloc(arg);
				if (byRef)
					il.Ldloca(arg);
				else
					il.Ldloc(arg);
			}

			il.CallOrCallvirt(type, method);

			for (var i = 0; i < parameters.Length; i++)
			{
				var parameterType = parameters[i].ParameterType;
				if (!parameterType.IsByRef)
					continue;

				il.Ldarg_1(); // args
				il.Ldc_I4(i); // i
				il.Ldloc(i + (method.IsStatic ? 0 : 1)); // args[i]

				parameterType = parameterType.GetElementType() ?? parameterType;
				if (parameterType.IsValueType)
					il.Box(parameterType);
				il.Stelem_Ref();
			}

			if (method.ReturnType == typeof(void))
				il.Ldnull();
			else
				il.MaybeBox(method.ReturnType);

			il.Ret();
		}

		#endregion
	}
}