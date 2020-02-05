// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen.Serialization
{
	public interface IReadObjectSink
	{
		void StartedReadingObject(Type type, AccessorMembers members);
		void ReadMember(Type parentType, string memberName, Type memberType, object memberValue);
	}
}