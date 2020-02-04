// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

namespace TypeKitchen.Scripting
{
	public static class ComputedExpressions
	{
		internal static string ResolveExpression(object @this, string expression, bool inline = false)
		{
			bool IsQuoted(Type type)
			{
				return !inline && (type == typeof(string) || type == typeof(StringValues) || type == typeof(char));
			}

			var accessor = ReadAccessor.Create(@this.GetType(), out var members);
			foreach (Match match in Regex.Matches(expression, @"{{([a-zA-Z\.,\""()\s]+)}}",
				RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled))
			{
				var keys = match.Groups[1].Value.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries);

				if (keys.Length == 1)
				{
					var key = keys[0];

					if (members.TryGetValue(key, out var propertyMember) &&
					    accessor.TryGetValue(@this, key, out var value))
						expression = expression.Replace(match.Groups[0].Value,
							IsQuoted(propertyMember.Type) ? $"\"{value}\"" : $"{value}");
					else if (key.Contains("("))
					{
						var callMembers = AccessorMembers.Create(@this,
							AccessorMemberTypes.Methods, AccessorMemberScope.Public);
						foreach (var member in callMembers)
							if (key.StartsWith(member.Name) && member.MemberInfo is MethodInfo method)
							{
								var caller = CallAccessor.Create(method);
								var result = caller.Call(@this, new object[0]); // FIXME: parse parameters
								var resultType = result.GetType();
								expression = expression.Replace(match.Groups[0].Value,
									IsQuoted(resultType) ? $"\"{result}\"" : $"{result}");
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