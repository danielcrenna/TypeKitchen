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
	public class ReadAccessorBenchmarks
	{
		private TypeAccessor _fastMember;
		private ITypeReadAccessor _typeKitchen;
		private OnePropertyOneFieldStrings _target;

		[GlobalSetup]
		public void GlobalSetup()
		{
			_fastMember = TypeAccessor.Create(typeof(OnePropertyOneFieldStrings));
			_typeKitchen = ReadAccessor.Create(typeof(OnePropertyOneFieldStrings));
			_target = new OnePropertyOneFieldStrings {Foo = "Bar", Bar = "Baz"};
		}

		[Benchmark(Baseline = true)]
		public void Contrived_Direct_Access()
		{
			var bar = DirectReadAccessorForOnePropertyOneField.Instance[_target, nameof(OnePropertyOneFieldStrings.Foo)];
			var baz = DirectReadAccessorForOnePropertyOneField.Instance[_target, nameof(OnePropertyOneFieldStrings.Bar)];
		}

		[Benchmark(Baseline = false)]
		public void TypeKitchen_ReadAccessor_Singleton()
		{
			var bar = _typeKitchen[_target, nameof(OnePropertyOneFieldStrings.Foo)];
			var baz = _typeKitchen[_target, nameof(OnePropertyOneFieldStrings.Bar)];
		}

		[Benchmark(Baseline = false)]
		public void FastMember_TypeAccessor_Singleton()
		{
			var bar = _fastMember[_target, nameof(OnePropertyOneFieldStrings.Foo)];
			var baz = _fastMember[_target, nameof(OnePropertyOneFieldStrings.Bar)];
		}
	}
}