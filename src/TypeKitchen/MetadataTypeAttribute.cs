// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class MetadataTypeAttribute : Attribute
	{
		public MetadataTypeAttribute(Type metadataType) => MetadataType = metadataType;

		public Type MetadataType { get; set; }
	}
}