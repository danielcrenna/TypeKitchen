// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using BenchmarkDotNet.Attributes;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(false, true)]
    [CsvMeasurementsExporter]
    [RPlotExporter]
    public class SingletonTypeResolverBenchmarks
    {
        private DirectTypeResolver _direct;
        private OnePropertyOneField _instance;
        private IContainer _autofacInstancedSingleton;
        private IContainer _autofacFunctionSingleton;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _instance = new OnePropertyOneField();

            _direct = new DirectTypeResolver();
            
            var directBuilder = new ContainerBuilder();
            directBuilder.RegisterInstance(_instance).SingleInstance();
            _autofacInstancedSingleton = directBuilder.Build();

            var indirectBuilder = new ContainerBuilder();
            indirectBuilder.Register(c => new OnePropertyOneField()).SingleInstance();
            _autofacFunctionSingleton = indirectBuilder.Build();
        }

        [Benchmark(Baseline = false)]
        public void Autofac_Direct()
        {
            _autofacInstancedSingleton.Resolve<OnePropertyOneField>();
        }

        [Benchmark(Baseline = false)]
        public void Autofac_Indirect()
        {
            _autofacFunctionSingleton.Resolve<OnePropertyOneField>();
        }

        [Benchmark(Baseline = true)]
        public void Native_Direct()
        {
            _direct.Resolve<OnePropertyOneField>();
        }

        [Benchmark(Baseline = false)]
        public void Native_Indirect()
        {
            _direct.Singleton(r => new OnePropertyOneField());
        }
    }
}