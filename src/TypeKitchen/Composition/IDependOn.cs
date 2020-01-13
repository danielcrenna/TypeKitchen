// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen.Composition
{
	/// <summary>
	/// The system <see cref="T"/> must execute before this <see cref="ISystem"/> executes.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	// ReSharper disable once UnusedTypeParameter (accessed via reflection)
	public interface IDependOn<T> where T : ISystem { } 
}