// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Composition
{
	public interface ISystem
	{
		
	}

	public interface ISystem<T1> : ISystem where T1 : struct
	{
		void Update(ref T1 component1);
	}

	public interface ISystem<T1, T2> : ISystem where T1 : struct where T2 : struct
	{
		void Update(ref T1 component1, ref T2 component2);
	}
}