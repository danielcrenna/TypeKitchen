﻿// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen.Composition
{
	public interface IComponent { }

    public interface IComponent<T> : IComponent where T : struct
	{
        T Value { get; set; }
	}
}