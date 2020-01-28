// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TypeKitchen.StateMachine
{
	[Serializable]
	public sealed class UnusedStateMethodsException : Exception
	{
		public UnusedStateMethodsException(ICollection<MethodInfo> stateMethods) : base(
			"State methods were unused (probably a naming error or undefined state):\n" +
			string.Join("\n", stateMethods)) =>
			StateMethods = new ReadOnlyCollection<string>(stateMethods.Select(x => x.Name).ToList());

		public UnusedStateMethodsException(SerializationInfo info, StreamingContext context)
			: base(info, context) =>
			StateMethods =
				info.GetValue(nameof(StateMethods), typeof(ReadOnlyCollection<string>)) as ReadOnlyCollection<string>;

		public ReadOnlyCollection<string> StateMethods { get; }

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException(nameof(info));

			info.AddValue(nameof(StateMethods), StateMethods);
			base.GetObjectData(info, context);
		}
	}
}