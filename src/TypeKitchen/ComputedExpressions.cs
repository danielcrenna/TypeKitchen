// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TypeKitchen
{
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
						var members = AccessorMembers.Create(@this, AccessorMemberScope.Public,
							AccessorMemberTypes.Methods);
						foreach (var member in members)
							if (key.StartsWith(member.Name) && member.MemberInfo is MethodInfo method)
							{
								var caller = CallAccessor.Create(method);
								var result = caller.Call(@this, new object[0]); // FIXME: parse parameters
								var replacer = result?.ToString();
								expression = expression.Replace(match.Groups[0].Value,
									quoted ? $"\"{replacer}\"" : replacer);
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