// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.StateMachine
{
	[Serializable]
	public class DuplicateStateMethodException : Exception
	{
		public DuplicateStateMethodException(params string[] stateMethods) : base(
			"Duplicate state methods were found: \n" + string.Join("\n", stateMethods))
		{
		}
	}
}