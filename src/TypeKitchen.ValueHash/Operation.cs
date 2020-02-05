// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace TypeKitchen.ValueHash
{
	public class Operation
	{
		public string Type { get; }
		public string Path { get; }
		public string From { get; }
		public object Value { get; }

		public Operation(string type, string path, string from, object value)
		{
			Type = type;
			Path = path;
			From = from;
			Value = value;
		}
	}
}