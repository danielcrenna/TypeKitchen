// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TypeKitchen.Serialization
{
	public sealed class JsonStringSerializer : IStringSerializer
	{
		private static readonly JsonSerializerOptions Settings;

		static JsonStringSerializer()
		{
			Settings = new JsonSerializerOptions
			{
				IgnoreNullValues = true,
				AllowTrailingCommas = true,
				IgnoreReadOnlyProperties = false,
				ReadCommentHandling = JsonCommentHandling.Skip,
				PropertyNameCaseInsensitive = true
			};

			//Settings.Converters.Add(new DynamicJsonConverter());
		}

		public ReadOnlySpan<byte> ToBuffer(string text, IObjectSerializer objectSerializer, ITypeResolver typeResolver)
		{
			var hash = JsonSerializer.Deserialize<Dictionary<string, object>>(text, Settings);
			var instance = Shaping.ToAnonymousObject(hash);
			var buffer = objectSerializer.ToBuffer(instance, typeResolver);
			return buffer;
		}
	}
}