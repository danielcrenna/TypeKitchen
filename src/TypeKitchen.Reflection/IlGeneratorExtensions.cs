// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace TypeKitchen.Reflection
{
	internal static class IlGeneratorExtensions
	{
		public static ILSugar GetILGeneratorInternal(this MethodBuilder b)
		{
			return new ILSugar(b.GetILGenerator());
		}

		public static ILSugar GetILGeneratorInternal(this DynamicMethod dm)
		{
			return new ILSugar(dm.GetILGenerator());
		}

		/// <summary>Pushes the value of i onto the evaluation stack as an int32.</summary>
		public static ILSugar LoadConstant(this ILSugar il, long i)
		{
			switch (i)
			{
				case 0:
					il.Ldc_I4_0();
					break;
				case 1:
					il.Ldc_I4_1();
					break;
				case 2:
					il.Ldc_I4_2();
					break;
				case 3:
					il.Ldc_I4_3();
					break;
				case 4:
					il.Ldc_I4_4();
					break;
				case 5:
					il.Ldc_I4_5();
					break;
				case 6:
					il.Ldc_I4_6();
					break;
				case 7:
					il.Ldc_I4_7();
					break;
				case 8:
					il.Ldc_I4_8();
					break;
				default:
					if (i <= byte.MaxValue)
						il.Ldc_I4_S((byte) i);
					else if (i <= int.MaxValue)
						il.Ldc_I4((int) i);
					else
						il.Ldc_I8(i);
					break;
			}

			return il;
		}

		/// <summary>Loads the local variable at index i onto the evaluation stack.</summary>
		public static ILSugar LoadVariable(this ILSugar il, int i)
		{
			switch (i)
			{
				case 0:
					il.Ldloc_0();
					break;
				case 1:
					il.Ldloc_1();
					break;
				case 2:
					il.Ldloc_2();
					break;
				case 3:
					il.Ldloc_3();
					break;
				default:
					if (i <= byte.MaxValue)
						il.Ldloc_S((byte) i);
					else
						il.Ldloc(i);
					break;
			}

			return il;
		}

		/// <summary>Loads the argument at index i onto the evaluation stack.</summary>
		public static ILSugar LoadArgument(this ILSugar il, int i)
		{
			switch (i)
			{
				case 0:
					il.Ldarg_0();
					break;
				case 1:
					il.Ldarg_1();
					break;
				case 2:
					il.Ldarg_2();
					break;
				case 3:
					il.Ldarg_3();
					break;
				default:
					if (i <= byte.MaxValue)
						il.Ldarg_S((byte) i);
					else
						il.Ldarg(i);
					break;
			}

			return il;
		}

		/// <summary>If the given type is a value type, converts it into an object reference (type O)</summary>
		public static ILSugar MaybeBox(this ILSugar il, Type type)
		{
			if (type.IsValueType)
				il.Box(type);
			return il;
		}

		/// <summary> Branches to the specified label if a string on the evaluation stacks equals the given literal value.</summary>
		public static ILSugar GotoIfStringEquals(this ILSugar il, string literal, Label @goto)
		{
			var ifStringEquals = il.Ldstr(literal).Call(KnownMethods.StringEquals);
			ifStringEquals
				.Brtrue(@goto); // TODO calculate offsets dynamically to call Brtrue_S vs. Brtrue, not worth it right now.
			return il;
		}

		public static ILSugar CastOrUnbox(this ILSugar il, Type type)
		{
			if (type == typeof(object))
				return il;

			if (type.IsValueType)
				il.Unbox(type);
			else
				il.Castclass(type);

			return il;
		}

		public static ILSugar CastOrUnboxAny(this ILSugar il, Type type)
		{
			if (type == typeof(object))
				return il;

			if (type.IsValueType)
				il.Unbox_Any(type);
			else
				il.Castclass(type);

			return il;
		}

		public static ILSugar CallOrCallvirt(this ILSugar il, Type type, MethodInfo method)
		{
			if (method.IsVirtual && !type.IsValueType)
				il.Callvirt(method);
			else
				il.Call(method);

			return il;
		}

		/// <summary> Creates a property with the specified name and a fixed reflection member value, known at runtime.</summary>
		public static void MemberProperty(this TypeBuilder tb, string propertyName, MemberInfo value,
			MethodInfo overrides = null)
		{
			Type type;
			switch (value)
			{
				case Type _:
					type = typeof(Type);
					break;
				case MethodInfo _:
					type = typeof(MethodInfo);
					break;
				case FieldInfo _:
					type = typeof(FieldInfo);
					break;
				case ConstructorInfo _:
					type = typeof(ConstructorInfo);
					break;
				default:
					throw new ArgumentException();
			}

			var getMethod = tb.DefineMethod($"get_{propertyName}",
				MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
				MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.SpecialName, type,
				Type.EmptyTypes);

			if (overrides != null)
				tb.DefineMethodOverride(getMethod, overrides);

			var propertyWithGet = tb.DefineProperty(propertyName, PropertyAttributes.None, type, Type.EmptyTypes);
			propertyWithGet.SetGetMethod(getMethod);

			var il = getMethod.GetILGeneratorInternal();
			il.Ldtoken(value);

			switch (value)
			{
				case Type _:
					il.Call(KnownMethods.GetTypeFromHandle);
					break;
				case ConstructorInfo _:
				case MethodInfo _:
					il.Call(KnownMethods.GetMethodFromHandle);
					break;
				case FieldInfo _:
					il.Call(KnownMethods.GetFieldFromHandle);
					break;
				default:
					throw new ArgumentException();
			}

			il.Ret();
		}
	}
}