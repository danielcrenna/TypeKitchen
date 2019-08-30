// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Composition
{
	public interface ISystem
	{
		
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	public interface ISystem<T1> : ISystem where T1 : struct
	{
		void Update(ref T1 component1);
	}

	/// <summary>
	/// A function that executes on all entities that own the specified component data.
	/// </summary>
	/// <typeparam name="T1">The first type of required component data.</typeparam>
	/// <typeparam name="T2">The second type of required component data. </typeparam>
	public interface ISystem<T1, T2> : ISystem where T1 : struct where T2 : struct
	{
		void Update(ref T1 component1, ref T2 component2);
	}
}