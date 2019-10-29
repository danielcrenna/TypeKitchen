// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using ExternalTestAssembly;
using Xunit;
using Xunit.Abstractions;

namespace TypeKitchen.Tests
{
	public class AssemblyHashTests
	{
		private readonly ITestOutputHelper _console;

		public AssemblyHashTests(ITestOutputHelper console)
		{
			_console = console;
		}

		[Fact]
		public void Can_get_build_independent_hash()
		{
			string lastIndependent = null;
			string lastDependent = null;

			if (File.Exists("LastAssemblyHash.txt"))
			{
				var lines = File.ReadAllLines("LastAssemblyHash.txt");
				lastIndependent = lines[0];
				lastDependent = lines[1];
			}

			var assembly = typeof(AnonymousTypeFactory).Assembly;

			var independent = BitConverter.ToString(AssemblyHash.GetBuildIndependent(assembly));
			Assert.NotNull(independent);
			_console.WriteLine(independent);

			var dependent = BitConverter.ToString(AssemblyHash.GetBuildDependent(assembly));
			Assert.NotNull(dependent);
			_console.WriteLine(dependent);

			Assert.NotEqual(independent, dependent);

			try
			{
				if (lastIndependent != null)
					Assert.Equal(lastIndependent, independent);

				//if (lastDependent != null)
				//	Assert.NotEqual(lastDependent, dependent);

				if(lastIndependent == null || lastDependent == null)
					_console.WriteLine($"Rebuild {assembly.GetName().Name} and re-run this test.");
			}
			finally
			{
				File.WriteAllLines("LastAssemblyHash.txt", new [] { independent, dependent});
			}
		}
	}
}
