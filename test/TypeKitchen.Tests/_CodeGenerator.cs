// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Humanizer;
using Xunit;
using Xunit.Abstractions;

namespace TypeKitchen.Tests
{
	public class CodeGenerator
	{
		private readonly ITestOutputHelper _console;

		public CodeGenerator(ITestOutputHelper console)
		{
			_console = console;
		}

		/*
			public uint CreateEntity<T1, T2, T3, T4, T5>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct
			{
				return CreateEntity(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
			}
         */

		[Fact]
		public void GenerateCreateEntityTypes()
		{
			var code = TypeKitchen.Pooling.StringBuilderPool.Scoped(sb =>
			{
				for (var i = 1; i <= 12; i++)
				{
					sb.AppendLine("/// <summary>");
					sb.AppendLine("/// Create a new entity possessing the specified component data.");
					sb.AppendLine("/// </summary>");

					for (var j = 1; j < i; j++)
					{
						sb.AppendLine($"/// <typeparam name=\"T{j}\">The {j.ToOrdinalWords()} type of declared component data.</typeparam>");
					}

					sb.Append("public uint CreateEntity<");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(", ");
						sb.Append($"T{j}");
					}
					sb.Append(">() ");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(" ");
						sb.Append($"where T{j} : struct");
					}
					sb.AppendLine();


					sb.AppendLine("{");

					sb.Append("    return CreateEntity((object) ");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(", ");
						sb.Append($"typeof(T{j})");
					}
					sb.AppendLine(");");

					sb.AppendLine("}");
					sb.AppendLine();
				}
			});

			_console.WriteLine(code);
		}

		[Fact]
		public void GenerateCreateEntityInstances()
		{
			var code = TypeKitchen.Pooling.StringBuilderPool.Scoped(sb =>
			{
				for (var i = 1; i <= 12; i++)
				{
					sb.AppendLine("/// <summary>");
					sb.AppendLine("/// Create a new entity possessing the specified component data.");
					sb.AppendLine("/// </summary>");

					for (var j = 1; j < i; j++)
					{
						sb.AppendLine($"/// <typeparam name=\"T{j}\">The {j.ToOrdinalWords()} type of declared component data.</typeparam>");
					}

					sb.Append("public uint CreateEntity<");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(", ");
						sb.Append($"T{j}");
					}
					sb.Append(">(");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(", ");
						sb.Append($"T{j} component{j}");
					}
                    sb.Append(") ");
                    for (var j = 1; j < i; j++)
                    {
	                    if (j != 1)
		                    sb.Append(" ");
	                    sb.Append($"where T{j} : struct");
                    }
                    sb.AppendLine();


					sb.AppendLine("{");

                    sb.Append("    return CreateEntity((object) ");
                    for (var j = 1; j < i; j++)
                    {
	                    if (j != 1)
		                    sb.Append(", ");
	                    sb.Append($"component{j}");
                    }
                    sb.AppendLine(");");

                    sb.AppendLine("}");
                    sb.AppendLine();
				}
			});

            _console.WriteLine(code);
		}

		[Fact]
		public void GenerateSystemWithSignatures()
		{
			var code = TypeKitchen.Pooling.StringBuilderPool.Scoped(sb =>
			{
				for (var i = 1; i <= 12; i++)
				{
					sb.AppendLine("/// <summary>");
					sb.AppendLine("/// A function that executes on all entities that own the specified component data.");
					sb.AppendLine("/// </summary>");
					
					for (var j = 1; j < i; j++)
					{
						sb.AppendLine($"/// <typeparam name=\"T{j}\">The {j.ToOrdinalWords()} type of required component data.</typeparam>");
					}

					sb.Append("public interface ISystem<");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(", ");
						sb.Append($"T{j}");
					}
					sb.Append("> : ISystem ");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(" ");
						sb.Append($"where T{j} : struct");
					}
					sb.AppendLine();

					sb.AppendLine("{");

					sb.Append("    void Update(");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(", ");
						sb.Append($"ref T{j} component{j}");
					}
					sb.AppendLine(");");

					sb.AppendLine("}");
					sb.AppendLine();
				}
			});

			_console.WriteLine(code);
		}

		[Fact]
		public void GenerateSystemWithStateSignatures()
		{
			var code = TypeKitchen.Pooling.StringBuilderPool.Scoped(sb =>
			{
				for (var i = 1; i <= 12; i++)
				{
					sb.AppendLine("/// <summary>");
					sb.AppendLine("/// A function that executes on all entities that own the specified component data.");
					sb.AppendLine("/// </summary>");
					sb.AppendLine("/// <typeparam name=\"TState\">The provided state to pass to the system if provided in the system update.</typeparam>");

					for (var j = 1; j < i; j++)
					{
						sb.AppendLine($"/// <typeparam name=\"T{j}\">The {j.ToOrdinalWords()} type of required component data.</typeparam>");
					}

					sb.Append("public interface ISystemWithState<in TState, ");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(", ");
						sb.Append($"T{j}");
					}
					sb.Append("> : ISystemWithState ");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(" ");
						sb.Append($"where T{j} : struct");
					}
					sb.AppendLine();

					sb.AppendLine("{");

					sb.Append("    void Update(TState state, ");
					for (var j = 1; j < i; j++)
					{
						if (j != 1)
							sb.Append(", ");
						sb.Append($"ref T{j} component{j}");
					}
					sb.AppendLine(");");

					sb.AppendLine("}");
                    sb.AppendLine();
				}
			});
			
			_console.WriteLine(code);
		}
	}
}
