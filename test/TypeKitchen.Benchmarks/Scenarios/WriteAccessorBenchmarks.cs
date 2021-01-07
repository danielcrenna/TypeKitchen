// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FastMember;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	[MemoryDiagnoser]
	[DisassemblyDiagnoser(exportDiff: true)]
	[CsvMeasurementsExporter]
	[RPlotExporter]
	public class WriteAccessorBenchmarks
	{
		private TypeAccessor _fastMember;
		private ITypeWriteAccessor _typeKitchen;

		[GlobalSetup]
		public void GlobalSetup()
		{
			_fastMember = TypeAccessor.Create(typeof(OnePropertyOneFieldStrings));
			_typeKitchen = WriteAccessor.Create(typeof(OnePropertyOneFieldStrings));
		}

		[Benchmark(Baseline = false)]
		public void FastMember_TypeAccessor_Singleton()
		{
			var target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};
			_fastMember[target, "Foo"] = "Fizz";
			_fastMember[target, "Bar"] = "Buzz";
		}

		[Benchmark(Baseline = false)]
		public void TypeKitchen_WriteAccessor_Singleton()
		{
			var target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};
			_typeKitchen[target, "Foo"] = "Fizz";
			_typeKitchen[target, "Bar"] = "Buzz";
		}

		[Benchmark(Baseline = false)]
		public void FastMember_TypeAccessor_Create()
		{
			var target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};
			var accessor = TypeAccessor.Create(target.GetType());
			accessor[target, "Foo"] = "Fizz";
			accessor[target, "Bar"] = "Buzz";
		}

		[Benchmark(Baseline = false)]
		public void TypeKitchen_WriteAccessor_Create()
		{
			var target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};
			var accessor = WriteAccessor.Create(target.GetType());
			accessor[target, "Foo"] = "Fizz";
			accessor[target, "Bar"] = "Buzz";
		}

		[Benchmark(Baseline = true)]
		public void Contrived_Direct_Access()
		{
			var target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};
			var bar = DirectWriteAccessor.Instance[target, "Foo"] = "Fizz";
			var baz = DirectWriteAccessor.Instance[target, "Bar"] = "Buzz";
		}
	}
}