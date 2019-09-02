// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Scripting;

namespace TypeKitchen
{
	public class ReferenceBuilder
	{
		private ScriptOptions _options;

		public ReferenceBuilder(ScriptOptions options)
		{
			_options = options;
		}

		public ReferenceBuilder Add(Type type)
		{
			_options = _options.AddReferences(type.Assembly);
			_options = _options.AddImports(type.Namespace);
			return this;
		}

		public ReferenceBuilder Add<T>()
		{
			return Add(typeof(T));
		}

		public ScriptOptions Build()
		{
			return _options;
		}
	}
}