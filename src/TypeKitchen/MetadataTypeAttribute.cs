// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace TypeKitchen
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public class MetadataTypeAttribute : Attribute
	{
		public MetadataTypeAttribute(string profile, Type metadataType)
		{
			Profile = profile;
			MetadataType = metadataType;
		}

		public string Profile { get; }

		public MetadataTypeAttribute(Type metadataType) : this("Default", metadataType) { }

		public Type MetadataType { get; }
	}
}