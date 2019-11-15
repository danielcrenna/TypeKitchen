// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;

namespace TypeKitchen
{
#if !LIGHT
	public class ReferenceBuilder
	{
		private readonly HashSet<Type> _visited;
		private ScriptOptions _options;
		
		public ReferenceBuilder(ScriptOptions options)
		{
			_options = options;
			_visited = new HashSet<Type>();
		}

		public ReferenceBuilder Add(Type type)
		{
			if (_visited.Contains(type))
				return this;
			_options = _options.AddReferences(type.Assembly);
			_options = _options.AddImports(type.Namespace);
			_visited.Add(type);
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
#endif
}