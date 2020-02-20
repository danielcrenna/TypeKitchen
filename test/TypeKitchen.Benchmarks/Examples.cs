// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Running;
using TypeKitchen.Benchmarks.Scenarios;

namespace TypeKitchen.Benchmarks
{
	internal class Examples
	{
		internal class SimpleType
		{
			public string ThisIsAString { get; set; }
		}


		private static void HelloWorld()
		{
			BenchmarkRunner.Run<ReadAccessorBenchmarks>();

			// Some structure, known in advance or at runtime
			var instance = new SimpleType { ThisIsAString = "Yep!" };

			//
			// Direct access (known in advance):
			Console.WriteLine(instance.ThisIsAString);

			//
			// Implicit access (known at runtime):
			Console.WriteLine(ReadAccessor.Create(typeof(SimpleType))[instance, nameof(SimpleType.ThisIsAString)]);

			if(Environment.UserInteractive)
				Console.ReadKey();
		}
	}
}