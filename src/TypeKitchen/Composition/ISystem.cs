// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen.Composition
{
	public interface ISystem { }

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	public interface ISystem<T1> : ISystem where T1 : struct
	{
		bool Update(UpdateContext context, ref T1 component1);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	public interface ISystem<T1, T2> : ISystem where T1 : struct where T2 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3> : ISystem where T1 : struct where T2 : struct where T3 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	/// <typeparam name="T4">The fourth type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3, T4> : ISystem where T1 : struct where T2 : struct where T3 : struct where T4 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	/// <typeparam name="T4">The fourth type of required component data.</typeparam>
	/// <typeparam name="T5">The fifth type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3, T4, T5> : ISystem where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	/// <typeparam name="T4">The fourth type of required component data.</typeparam>
	/// <typeparam name="T5">The fifth type of required component data.</typeparam>
	/// <typeparam name="T6">The sixth type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3, T4, T5, T6> : ISystem where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	/// <typeparam name="T4">The fourth type of required component data.</typeparam>
	/// <typeparam name="T5">The fifth type of required component data.</typeparam>
	/// <typeparam name="T6">The sixth type of required component data.</typeparam>
	/// <typeparam name="T7">The seventh type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3, T4, T5, T6, T7> : ISystem where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	/// <typeparam name="T4">The fourth type of required component data.</typeparam>
	/// <typeparam name="T5">The fifth type of required component data.</typeparam>
	/// <typeparam name="T6">The sixth type of required component data.</typeparam>
	/// <typeparam name="T7">The seventh type of required component data.</typeparam>
	/// <typeparam name="T8">The eighth type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3, T4, T5, T6, T7, T8> : ISystem where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	/// <typeparam name="T4">The fourth type of required component data.</typeparam>
	/// <typeparam name="T5">The fifth type of required component data.</typeparam>
	/// <typeparam name="T6">The sixth type of required component data.</typeparam>
	/// <typeparam name="T7">The seventh type of required component data.</typeparam>
	/// <typeparam name="T8">The eighth type of required component data.</typeparam>
	/// <typeparam name="T9">The ninth type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3, T4, T5, T6, T7, T8, T9> : ISystem where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	/// <typeparam name="T4">The fourth type of required component data.</typeparam>
	/// <typeparam name="T5">The fifth type of required component data.</typeparam>
	/// <typeparam name="T6">The sixth type of required component data.</typeparam>
	/// <typeparam name="T7">The seventh type of required component data.</typeparam>
	/// <typeparam name="T8">The eighth type of required component data.</typeparam>
	/// <typeparam name="T9">The ninth type of required component data.</typeparam>
	/// <typeparam name="T10">The tenth type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : ISystem where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data.</typeparam>
	/// <typeparam name="T3">The third type of required component data.</typeparam>
	/// <typeparam name="T4">The fourth type of required component data.</typeparam>
	/// <typeparam name="T5">The fifth type of required component data.</typeparam>
	/// <typeparam name="T6">The sixth type of required component data.</typeparam>
	/// <typeparam name="T7">The seventh type of required component data.</typeparam>
	/// <typeparam name="T8">The eighth type of required component data.</typeparam>
	/// <typeparam name="T9">The ninth type of required component data.</typeparam>
	/// <typeparam name="T10">The tenth type of required component data.</typeparam>
	/// <typeparam name="T11">The eleventh type of required component data.</typeparam>
	public interface ISystem<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : ISystem where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct where T6 : struct where T7 : struct where T8 : struct where T9 : struct where T10 : struct where T11 : struct
	{
		bool Update(UpdateContext context, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4, ref T5 component5, ref T6 component6, ref T7 component7, ref T8 component8, ref T9 component9, ref T10 component10, ref T11 component11);
	}
}