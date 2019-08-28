// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.StateMachine
{
	[Serializable]
	public class AlreadyInitializedException : Exception
	{
		public AlreadyInitializedException() : base(
			"StateProvider was already setup, and clear was not called before calling setup again")
		{
		}
	}
}