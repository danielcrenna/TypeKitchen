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
    public class WriteAccessorBenchmarks
    {
        private TypeAccessor _fastMember;
        private ITypeWriteAccessor _typeKitchen;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fastMember = TypeAccessor.Create(typeof(OnePropertyOneField));
            _typeKitchen = WriteAccessor.Create(typeof(OnePropertyOneField));
        }

        [Benchmark(Baseline = false)]
        public void FastMember_TypeAccessor_Singleton()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            _fastMember[target, "Foo"] = "Fizz";
            _fastMember[target, "Bar"] = "Buzz";
        }

        [Benchmark(Baseline = false)]
        public void TypeKitchen_WriteAccessor_Singleton()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            _typeKitchen[target, "Foo"] = "Fizz";
            _typeKitchen[target, "Bar"] = "Buzz";
        }

        [Benchmark(Baseline = false)]
        public void FastMember_TypeAccessor_Create()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var accessor = TypeAccessor.Create(target.GetType());
            accessor[target, "Foo"] = "Fizz";
            accessor[target, "Bar"] = "Buzz";
        }

        [Benchmark(Baseline = false)]
        public void TypeKitchen_WriteAccessor_Create()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var accessor = WriteAccessor.Create(target.GetType());
            accessor[target, "Foo"] = "Fizz";
            accessor[target, "Bar"] = "Buzz";
        }

        [Benchmark(Baseline = true)]
        public void Contrived_Direct_Access()
        {
            var target = new OnePropertyOneField {Foo = "Bar", Bar = "Baz"};
            var bar = DirectWriteAccessorForOnePropertyOneField.Instance[target, "Foo"] = "Fizz";
            var baz = DirectWriteAccessorForOnePropertyOneField.Instance[target, "Bar"] = "Buzz";
        }
    }
}