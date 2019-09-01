// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Composition
{
	// FIXME: This still boxes: https://stackoverflow.com/questions/3032750/structs-interfaces-and-boxing
	public interface IComponentProxy
	{
		Type RefType { get; }
	}
}