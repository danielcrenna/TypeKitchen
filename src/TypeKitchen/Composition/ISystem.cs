// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace TypeKitchen.Composition
{
	public interface ISystem
	{
		
	}

	public interface ISystem<T> : ISystem
	{
		void Update(ref T component1);
	}

	public interface ISystem<T1, T2> : ISystem
	{
		void Update(ref T1 component1, ref T2 component2);
	}
}