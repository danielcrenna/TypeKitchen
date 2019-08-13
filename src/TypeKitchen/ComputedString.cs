// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Scripting;

namespace TypeKitchen
{
	public static class ComputedPredicate
	{
		private static readonly ScriptOptions Options;

		static ComputedPredicate()
		{
			Options = ScriptOptions.Default
				.WithReferences(typeof(Console).Assembly, typeof(MemberExpression).Assembly)
				.WithImports("System", "System.Text", "System.Linq", "System.Collections.Generic");
		}

		public static bool Compute(object @this, string expression)
		{
			//
			// Pass 1: Resolve any {{ Member }} against self.
			var code = Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append($"public static bool Method() {{ return {ComputedExpressions.ResolveExpression(@this, expression, true)}; }}");
			});

			//
			// Pass 2: Execute script in context.
			var binding = LateBinding.DynamicMethodBindCall(Snippet.CreateMethod(code, Options));
			return (bool) binding.Invoke(null, null);
		}

		public static bool Compute(string expression)
		{
			var binding = LateBinding.DynamicMethodBindCall(Snippet.CreateMethod($"public static bool Method() {{ return {expression}; }}", Options));
			return (bool) binding.Invoke(null, null);
		}
	}
	
	public static class ComputedString
	{
		private static readonly ScriptOptions Options;

		static ComputedString()
		{
			Options = ScriptOptions.Default
				.WithReferences(typeof(Console).Assembly, typeof(MemberExpression).Assembly)
				.WithImports("System", "System.Text", "System.Linq", "System.Collections.Generic");
		}

		public static string Compute(object @this, string expression)
		{
			//
			// Pass 1: Resolve any {{ Member }} against self.
			var code = Pooling.StringBuilderPool.Scoped(sb =>
			{
				sb.Append($"public static string Method() {{ return \"{ComputedExpressions.ResolveExpression(@this, expression, false)}\"; }}");
			});

			//
			// Pass 2: Execute script in context.
			var binding = LateBinding.DynamicMethodBindCall(Snippet.CreateMethod(code, Options));
			return binding.Invoke(null, null).ToString();
		}

		public static string Compute(string expression)
		{
			var binding = LateBinding.DynamicMethodBindCall(Snippet.CreateMethod($"public static string Method() {{ return \"{expression}\"; }}", Options));
			return binding.Invoke(null, null).ToString();
		}
	}

	public static class ComputedExpressions
	{
		internal static string ResolveExpression(object @this, string expression, bool quoted)
		{
			var accessor = ReadAccessor.Create(@this.GetType());
			foreach (Match match in Regex.Matches(expression, @"{{([a-zA-Z\.,\""()\s]+)}}",
				RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled))
			{
				var keys = match.Groups[1].Value.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries);

				if (keys.Length == 1)
				{
					var key = keys[0];

					if (accessor.TryGetValue(@this, key, out var value))
					{
						var replacer = value?.ToString();
						expression = expression.Replace(match.Groups[0].Value, quoted ? $"\"{replacer}\"" : replacer);
					}
					else if (key.Contains("("))
					{
						var members = AccessorMembers.Create(@this, AccessorMemberScope.Public, AccessorMemberTypes.Methods);
						foreach (var member in members)
						{
							if (key.StartsWith(member.Name) && member.MemberInfo is MethodInfo method)
							{
								var caller = CallAccessor.Create(method);
								var result = caller.Call(@this, new object[0]); // FIXME: parse parameters
								var replacer = result?.ToString();
								expression = expression.Replace(match.Groups[0].Value, quoted ? $"\"{replacer}\"" : replacer);
							}
						}
					}

					continue;
				}

				for (var i = 0; i < keys.Length; i++)
				{
					var key = keys[i];
					if (accessor.TryGetValue(@this, key, out var value))
					{
						var child = ComputedString.Compute(value, $"{{{{{keys[i + 1]}}}}}");
						expression = expression.Replace(match.Groups[0].Value, child);
						break;
					}
				}
			}

			return expression;
		}
	}

}