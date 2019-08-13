// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.Scripting;

namespace TypeKitchen
{
	public static class ComputedPredicate
	{
		private static readonly ScriptOptions Options;

		static ComputedPredicate() =>
			Options = ScriptOptions.Default
				.WithReferences(typeof(Console).Assembly, typeof(MemberExpression).Assembly)
				.WithImports("System", "System.Text", "System.Linq", "System.Collections.Generic");

		public static bool Compute(object @this, string expression)
		{
			//
			// Pass 1: Resolve any {{ Member }} against self.
			var code = Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append(
					$"public static bool Method() {{ return {ComputedExpressions.ResolveExpression(@this, expression, true)}; }}");
			});

			//
			// Pass 2: Execute script in context.
			var binding = LateBinding.DynamicMethodBindCall(Snippet.CreateMethod(code, Options));
			return (bool) binding.Invoke(null, null);
		}

		public static bool Compute(string expression)
		{
			var binding =
				LateBinding.DynamicMethodBindCall(
					Snippet.CreateMethod($"public static bool Method() {{ return {expression}; }}", Options));
			return (bool) binding.Invoke(null, null);
		}
	}
}