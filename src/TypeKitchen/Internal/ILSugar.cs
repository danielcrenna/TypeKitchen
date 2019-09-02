// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace TypeKitchen.Internal
{
	/// <summary>Provides typing efficiency, method chaining, and light argument hinting. Should not do anything "smart".</summary>
	internal class ILSugar
	{
		private readonly ILGenerator _il;

		public ILSugar(ILGenerator il) => _il = il;

		/// <summary>Declares a new label.</summary>
		public Label DefineLabel()
		{
			return _il.DefineLabel();
		}

		/// <summary>Declares a local variable of the specified type, optionally pinning the object referred to by the variable.</summary>
		public LocalBuilder DeclareLocal(Type type, bool pinned = false)
		{
			return _il.DeclareLocal(type, pinned);
		}

		/// <summary>Marks the Microsoft intermediate language (MSIL) stream's current position with the given label.</summary>
		public ILSugar MarkLabel(Label loc)
		{
			_il.MarkLabel(loc);
			return this;
		}

		/// <summary>Loads the argument at index 0 onto the evaluation stack.</summary>
		public ILSugar Ldarg_0()
		{
			_il.Emit(OpCodes.Ldarg_0);
			return this;
		}

		/// <summary>Loads the argument at index 1 onto the evaluation stack.</summary>
		public ILSugar Ldarg_1()
		{
			_il.Emit(OpCodes.Ldarg_1);
			return this;
		}

		/// <summary>Loads the argument at index 2 onto the evaluation stack.</summary>
		public ILSugar Ldarg_2()
		{
			_il.Emit(OpCodes.Ldarg_2);
			return this;
		}

		/// <summary>Loads the argument at index 3 onto the evaluation stack.</summary>
		public ILSugar Ldarg_3()
		{
			_il.Emit(OpCodes.Ldarg_3);
			return this;
		}

		/// <summary>Loads an argument (referenced by a specified index value) onto the stack.</summary>
		public ILSugar Ldarg(int index)
		{
			_il.Emit(OpCodes.Ldarg);
			return this;
		}

		/// <summary>Loads the argument (referenced by a specified short form index) onto the evaluation stack.</summary>
		public ILSugar Ldarg_S(int index)
		{
			_il.Emit(OpCodes.Ldarg_S);
			return this;
		}

		/// <summary>Pushes the integer value of 0 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_0()
		{
			_il.Emit(OpCodes.Ldc_I4_0);
			return this;
		}

		/// <summary>Pushes the integer value of 1 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_1()
		{
			_il.Emit(OpCodes.Ldc_I4_1);
			return this;
		}

		/// <summary>Pushes the integer value of 2 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_2()
		{
			_il.Emit(OpCodes.Ldc_I4_2);
			return this;
		}

		/// <summary>Pushes the integer value of 3 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_3()
		{
			_il.Emit(OpCodes.Ldc_I4_3);
			return this;
		}

		/// <summary>Pushes the integer value of 4 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_4()
		{
			_il.Emit(OpCodes.Ldc_I4_4);
			return this;
		}

		/// <summary>Pushes the integer value of 5 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_5()
		{
			_il.Emit(OpCodes.Ldc_I4_5);
			return this;
		}

		/// <summary>Pushes the integer value of 6 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_6()
		{
			_il.Emit(OpCodes.Ldc_I4_6);
			return this;
		}

		/// <summary>Pushes the integer value of 7 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_7()
		{
			_il.Emit(OpCodes.Ldc_I4_7);
			return this;
		}

		/// <summary>Pushes the integer value of 8 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_8()
		{
			_il.Emit(OpCodes.Ldc_I4_8);
			return this;
		}

		/// <summary>Pushes the integer value of -1 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4_M1()
		{
			_il.Emit(OpCodes.Ldc_I4_M1);
			return this;
		}

		/// <summary>Pushes a supplied value of type int32 onto the evaluation stack as an int32.</summary>
		public ILSugar Ldc_I4(int value)
		{
			_il.Emit(OpCodes.Ldc_I4, value);
			return this;
		}

		/// <summary>Pushes the supplied int8 value onto the evaluation stack as an int32, short form.</summary>
		public ILSugar Ldc_I4_S(byte value)
		{
			_il.Emit(OpCodes.Ldc_I4_S, (int) value);
			return this;
		}

		/// <summary>Pushes a supplied value of type int64 onto the evaluation stack as an int64.</summary>
		public ILSugar Ldc_I8(long value)
		{
			_il.Emit(OpCodes.Ldc_I8, value);
			return this;
		}

		/// <summary>Pushes a null reference (type O) onto the evaluation stack.</summary>
		public ILSugar Ldnull()
		{
			_il.Emit(OpCodes.Ldnull);
			return this;
		}

		/// <summary>Pushes a new object reference to a string literal stored in the metadata.</summary>
		public ILSugar Ldstr(string literal)
		{
			_il.Emit(OpCodes.Ldstr, literal);
			return this;
		}

		/// <summary>Finds the value of a field in the object whose reference is currently on the evaluation stack.</summary>
		public ILSugar Ldfld(FieldInfo field)
		{
			_il.Emit(OpCodes.Ldfld, field);
			return this;
		}

		/// <summary>Converts a metadata token to its runtime representation, pushing it onto the evaluation stack.</summary>
		public ILSugar Ldtoken(MemberInfo member)
		{
			switch (member)
			{
				case Type type:
					_il.Emit(OpCodes.Ldtoken, type);
					break;
				case MethodInfo method:
					_il.Emit(OpCodes.Ldtoken, method);
					break;
				case FieldInfo field:
					_il.Emit(OpCodes.Ldtoken, field);
					break;
				case ConstructorInfo ctor:
					_il.Emit(OpCodes.Ldtoken, ctor);
					break;
				default:
					throw new ArgumentException();
			}

			return this;
		}

		/// <summary>Pushes the value of a static field onto the evaluation stack.</summary>
		public ILSugar Ldsfld(FieldInfo field)
		{
			_il.Emit(OpCodes.Ldsfld, field);
			return this;
		}

		/// <summary>Replaces the value stored in the field of an object reference or pointer with a new value.</summary>
		public ILSugar Stfld(FieldInfo field)
		{
			_il.Emit(OpCodes.Stfld, field);
			return this;
		}

		/// <summary>Replaces the value of a static field with a value from the evaluation stack.</summary>
		public ILSugar Stsfld(FieldInfo field)
		{
			_il.Emit(OpCodes.Stsfld, field);
			return this;
		}

		/// <summary>Stores the value on top of the evaluation stack in the argument slot at a specified index, short form.</summary>
		public ILSugar Starg_S()
		{
			_il.Emit(OpCodes.Starg_S);
			return this;
		}

		/// <summary>
		///     Pops the current value from the top of the evaluation stack and stores it in the local variable list at a
		///     specified index.
		/// </summary>
		public ILSugar Stloc(LocalBuilder local)
		{
			_il.Emit(OpCodes.Stloc, local);
			return this;
		}

		/// <summary>
		///     Pops the current value from the top of the evaluation stack and stores it in the local variable list at index
		///     0.
		/// </summary>
		public ILSugar Stloc_0()
		{
			_il.Emit(OpCodes.Stloc_0);
			return this;
		}

		/// <summary>
		///     Pops the current value from the top of the evaluation stack and stores it in the local variable list at index
		///     1.
		/// </summary>
		public ILSugar Stloc_1()
		{
			_il.Emit(OpCodes.Stloc_1);
			return this;
		}

		/// <summary>
		///     Pops the current value from the top of the evaluation stack and stores it in the local variable list at index
		///     2.
		/// </summary>
		public ILSugar Stloc_2()
		{
			_il.Emit(OpCodes.Stloc_2);
			return this;
		}

		/// <summary>
		///     Pops the current value from the top of the evaluation stack and stores it in the local variable list at index
		///     3.
		/// </summary>
		public ILSugar Stloc_3()
		{
			_il.Emit(OpCodes.Stloc_3);
			return this;
		}

		/// <summary>
		///     Pops the current value from the top of the evaluation stack and stores it in the local variable list at
		///     <paramref name="index">index</paramref> (short form).
		/// </summary>
		public ILSugar Stloc_S(int index)
		{
			_il.Emit(OpCodes.Stloc_S, index);
			return this;
		}

		/// <summary>Copies the value type object pointed to by an address to the top of the evaluation stack.</summary>
		public ILSugar Ldobj(Type type)
		{
			_il.Emit(OpCodes.Ldobj, type);
			return this;
		}

		/// <summary>Loads an object reference as a type O (object reference) onto the evaluation stack indirectly.</summary>
		public ILSugar Ldind_Ref()
		{
			_il.Emit(OpCodes.Ldind_Ref);
			return this;
		}

		/// <summary>Loads the local variable at a specific index onto the evaluation stack.</summary>
		public ILSugar Ldloc(int index)
		{
			_il.Emit(OpCodes.Ldloc, index);
			return this;
		}

		/// <summary>Loads the local variable at index 0 onto the evaluation stack.</summary>
		public ILSugar Ldloc_0()
		{
			_il.Emit(OpCodes.Ldloc_0);
			return this;
		}

		/// <summary>Loads the local variable at index 1 onto the evaluation stack.</summary>
		public ILSugar Ldloc_1()
		{
			_il.Emit(OpCodes.Ldloc_1);
			return this;
		}

		/// <summary>Loads the local variable at index 2 onto the evaluation stack.</summary>
		public ILSugar Ldloc_2()
		{
			_il.Emit(OpCodes.Ldloc_2);
			return this;
		}

		/// <summary>Loads the local variable at index 3 onto the evaluation stack.</summary>
		public ILSugar Ldloc_3()
		{
			_il.Emit(OpCodes.Ldloc_3);
			return this;
		}

		/// <summary>Loads the local variable at a specific index onto the evaluation stack, short form.</summary>
		public ILSugar Ldloc_S(int index)
		{
			_il.Emit(OpCodes.Ldloc_S, index);
			return this;
		}

		/// <summary>Loads the address of the local variable at a specific index onto the evaluation stack.</summary>
		public ILSugar Ldloca(int index)
		{
			_il.Emit(OpCodes.Ldloca, index);
			return this;
		}

		/// <summary>Loads the address of the local variable at a specific index onto the evaluation stack, short form.</summary>
		public ILSugar Ldloca_S(byte index)
		{
			_il.Emit(OpCodes.Ldloca_S, index);
			return this;
		}

		/// <summary>Loads the address of the local variable at a specific index onto the evaluation stack.</summary>
		public ILSugar Ldloca(LocalBuilder local)
		{
			_il.Emit(OpCodes.Ldloca, local);
			return this;
		}

		/// <summary>Loads the address of the local variable at a specific index onto the evaluation stack.</summary>
		public ILSugar Ldloc(LocalBuilder local)
		{
			_il.Emit(OpCodes.Ldloc, local);
			return this;
		}

		/// <summary>Calls the method indicated by the passed method descriptor.</summary>
		public ILSugar Call(MethodInfo method)
		{
			_il.Emit(OpCodes.Call, method);
			return this;
		}

		/// <summary>Calls a late-bound method on an object, pushing the return value onto the evaluation stack.</summary>
		public ILSugar Callvirt(MethodInfo method)
		{
			_il.Emit(OpCodes.Callvirt, method);
			return this;
		}

		/// <summary>Attempts to cast an object passed by reference to the specified class.</summary>
		public ILSugar Castclass(Type type)
		{
			_il.Emit(OpCodes.Castclass, type);
			return this;
		}

		/// <summary>
		///     Creates a new object or a new instance of a value type, pushing an object reference (type O) onto the
		///     evaluation stack.
		/// </summary>
		public ILSugar Newobj(ConstructorInfo ctor)
		{
			_il.Emit(OpCodes.Newobj, ctor);
			return this;
		}

		/// <summary>
		///     Pushes an object reference to a new zero-based, one-dimensional array whose elements are of a specific type
		///     onto the evaluation stack.
		/// </summary>
		public ILSugar Newarr(Type type)
		{
			_il.Emit(OpCodes.Newarr, type);
			return this;
		}

		/// <summary>Throws the exception object currently on the evaluation stack.</summary>
		public ILSugar Throw()
		{
			_il.Emit(OpCodes.Throw);
			return this;
		}

		/// <summary>Unconditionally transfers control to a target instruction (short form).</summary>
		public ILSugar Br_S(Label label)
		{
			_il.Emit(OpCodes.Br_S, label);
			return this;
		}

		/// <summary>Unconditionally transfers control to a target instruction.</summary>
		public ILSugar Br(Label label)
		{
			_il.Emit(OpCodes.Br, label);
			return this;
		}

		/// <summary>
		///     Transfers control to a target instruction (short form) if <paramref name="value">value</paramref> is true, not
		///     null, or non-zero.
		/// </summary>
		public ILSugar Brtrue_S(Label value)
		{
			_il.Emit(OpCodes.Brtrue_S, value);
			return this;
		}

		/// <summary>
		///     Transfers control to a target instruction if <paramref name="value">value</paramref> is true, not null, or
		///     non-zero.
		/// </summary>
		public ILSugar Brtrue(Label value)
		{
			_il.Emit(OpCodes.Brtrue, value);
			return this;
		}

		/// <summary>Converts a value type to an object reference (type O).</summary>
		public ILSugar Box(Type type)
		{
			_il.Emit(OpCodes.Box, type);
			return this;
		}

		/// <summary>
		///     Returns from the current method, pushing a return value (if present) from the callee's evaluation stack onto
		///     the caller's evaluation stack.
		/// </summary>
		public ILSugar Ret()
		{
			_il.Emit(OpCodes.Ret);
			return this;
		}

		/// <summary>Stores an object reference value at a supplied address.</summary>
		public ILSugar Stind_Ref()
		{
			_il.Emit(OpCodes.Stind_Ref);
			return this;
		}

		/// <summary>
		///     Fills space if opcodes are patched. No meaningful operation is performed although a processing cycle can be
		///     consumed.
		/// </summary>
		public ILSugar Nop()
		{
			_il.Emit(OpCodes.Nop);
			return this;
		}

		/// <summary>Replaces the array element at a given index with the object ref value (type O) on the evaluation stack.</summary>
		public ILSugar Stelem_Ref()
		{
			_il.Emit(OpCodes.Stelem_Ref);
			return this;
		}

		/// <summary>Removes the value currently on top of the evaluation stack.</summary>
		public ILSugar Pop()
		{
			_il.Emit(OpCodes.Pop);
			return this;
		}

		/// <summary>
		///     Loads the element containing an object reference at a specified array index onto the top of the evaluation
		///     stack as type O (object reference).
		/// </summary>
		public ILSugar Ldelem_Ref()
		{
			_il.Emit(OpCodes.Ldelem_Ref);
			return this;
		}

		/// <summary>Converts the boxed representation of a value type to its unboxed form.</summary>
		public ILSugar Unbox(Type type)
		{
			_il.Emit(OpCodes.Unbox, type);
			return this;
		}

		/// <summary>Converts the boxed representation of a type specified in the instruction to its unboxed form.</summary>
		public ILSugar Unbox_Any(Type type)
		{
			_il.Emit(OpCodes.Unbox_Any, type);
			return this;
		}

		/// <summary> Puts a call instruction onto the Microsoft intermediate language (MSIL) stream to call a varargs method. </summary>
		public ILSugar EmitCall(MethodInfo method)
		{
			_il.EmitCall(OpCodes.Call, method, null);
			return this;
		}

		/// <summary> Puts a callvirt instruction onto the Microsoft intermediate language (MSIL) stream to call a varargs method. </summary>
		public ILSugar EmitCallvirt(MethodInfo method)
		{
			_il.EmitCall(OpCodes.Callvirt, method, null);
			return this;
		}

		/// <summary>Copies the current topmost value on the evaluation stack, and then pushes the copy onto the evaluation stack.</summary>
		public ILSugar Dup()
		{
			_il.Emit(OpCodes.Dup);
			return this;
		}
	}
}