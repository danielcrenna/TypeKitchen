// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace TypeKitchen.Composition
{
	public interface ISystem
	{
		
	}

	public interface ISystem<T> : ISystem where T : IComponent
	{
		void Update(ref T component);
	}

	public interface ISystem<T1, T2> : ISystem where T1 : IComponent where T2 : IComponent
	{
		void Update(ref T1 component1, ref T2 component2);
	}
}