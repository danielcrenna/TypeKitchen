// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	[MemoryDiagnoser]
	[DisassemblyDiagnoser(exportDiff: true)]
	[CsvMeasurementsExporter]
	[RPlotExporter]
	public class SingletonTypeResolverBenchmarks
	{
		private IContainer _autofacFunctionSingleton;
		private IContainer _autofacInstancedSingleton;
		private OnePropertyOneFieldStrings _instance;

		[GlobalSetup]
		public void GlobalSetup()
		{
			_instance = new OnePropertyOneFieldStrings();

			var directBuilder = new ContainerBuilder();
			directBuilder.RegisterInstance(_instance).SingleInstance();
			_autofacInstancedSingleton = directBuilder.Build();

			var indirectBuilder = new ContainerBuilder();
			indirectBuilder.Register(c => new OnePropertyOneFieldStrings()).SingleInstance();
			_autofacFunctionSingleton = indirectBuilder.Build();
		}

		[Benchmark(Baseline = false)]
		public void Autofac_Indirect()
		{
			_autofacFunctionSingleton.Resolve<OnePropertyOneFieldStrings>();
		}

		[Benchmark(Baseline = true)]
		public void Autofac_Direct()
		{
			_autofacInstancedSingleton.Resolve<OnePropertyOneFieldStrings>();
		}
	}
}