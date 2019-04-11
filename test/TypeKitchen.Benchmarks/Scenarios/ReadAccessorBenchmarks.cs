// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;
using FastMember;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(false, true)]
    [CsvMeasurementsExporter]
    [RPlotExporter]
    public class ReadAccessorBenchmarks
    {
        private TypeAccessor _fastMember;
        private ITypeReadAccessor _typeKitchen;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fastMember = TypeAccessor.Create(typeof(OnePropertyOneField));
            _typeKitchen = ReadAccessor.Create(typeof(OnePropertyOneField));
        }

        [Benchmark(Baseline = false)]
        public void FastMember_TypeAccessor_Singleton()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var bar = _fastMember[target, "Foo"];
            var baz = _fastMember[target, "Bar"];
        }

        [Benchmark(Baseline = false)]
        public void TypeKitchen_ReadAccessor_Singleton()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var bar = _typeKitchen[target, "Foo"];
            var baz = _typeKitchen[target, "Bar"];
        }

        [Benchmark(Baseline = false)]
        public void FastMember_TypeAccessor_Create()
        {
            var target = new OnePropertyOneField { Foo = "Bar", Bar = "Baz" };
            var accessor = TypeAccessor.Create(target.GetType());
            var bar = accessor[target, "Foo"];
            var baz = accessor[target, "Bar"];
        }

        [Benchmark(Baseline = false)]
        public void TypeKitchen_ReadAccessor_Create()
        {
            var target = new OnePropertyOneField { Foo = "Bar", Bar = "Baz" };
            var accessor = ReadAccessor.Create(target.GetType());
            var bar = accessor[target, "Foo"];
            var baz = accessor[target, "Bar"];
        }

        [Benchmark(Baseline = true)]
        public void Contrived_Direct_Access()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var bar = DirectReadAccessorForOnePropertyOneField.Instance[target, "Foo"];
            var baz = DirectReadAccessorForOnePropertyOneField.Instance[target, "Bar"];
        }
    }
}