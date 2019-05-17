// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Attributes;
using DotLiquid;
using FastMember;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(false, true)]
    [CsvMeasurementsExporter]
    [RPlotExporter]
    public class ReadAccessorAnonymousTypeBenchmarks
    {
        private TypeAccessor _fastMember;
        private Func<object, object> _getBar;
        private Func<object, object> _getFoo;
        private Func<object, string, object> _indirect;
        private ITypeReadAccessor _typeKitchen;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var prototype = new {Foo = "Bar", Bar = "Baz"};
            _indirect = DirectAnonymousReadAccessor.Instance.SimulateIndirectAccess();
            _getFoo = DirectAnonymousReadAccessor.Instance.SimulateNonBranchingIndirectAccess("Foo");
            _getBar = DirectAnonymousReadAccessor.Instance.SimulateNonBranchingIndirectAccess("Bar");
            _fastMember = TypeAccessor.Create(prototype.GetType());
            _typeKitchen = ReadAccessor.Create(prototype.GetType());
        }

        [Benchmark(Baseline = false)]
        public void FastMember_TypeAccessor_Singleton()
        {
            var target = new {Foo = "Bar", Bar = "Baz"};
            var bar = _fastMember[target, "Foo"];
            var baz = _fastMember[target, "Bar"];
        }

        [Benchmark(Baseline = false)]
        public void TypeKitchen_ReadAccessor_Singleton()
        {
            var target = new {Foo = "Bar", Bar = "Baz"};
            var bar = _typeKitchen[target, "Foo"];
            var baz = _typeKitchen[target, "Bar"];
        }

        [Benchmark(Baseline = false)]
        public void FastMember_TypeAccessor_Create()
        {
            var target = new {Foo = "Bar", Bar = "Baz"};
            var accessor = TypeAccessor.Create(target.GetType());
            var bar = accessor[target, "Foo"];
            var baz = accessor[target, "Bar"];
        }

        [Benchmark(Baseline = false)]
        public void TypeKitchen_ReadAccessor_Create()
        {
            var target = new {Foo = "Bar", Bar = "Baz"};
            var accessor = ReadAccessor.Create(target.GetType());
            var bar = accessor[target, "Foo"];
            var baz = accessor[target, "Bar"];
        }

        [Benchmark(Baseline = false)]
        public void Contrived_Direct_Access()
        {
            var target = new TwoProperties {Foo = "Bar", Bar = "Baz"};
            var bar = DirectAnonymousReadAccessor.Instance[target, "Foo"];
            var baz = DirectAnonymousReadAccessor.Instance[target, "Bar"];
        }

        [Benchmark(Baseline = false)]
        public void Contrived_Indirect_Access()
        {
            var target = new TwoProperties {Foo = "Bar", Bar = "Baz"};
            var bar = _indirect(target, "Foo");
            var baz = _indirect(target, "Bar");
        }

        [Benchmark(Baseline = false)]
        public void Contrived_Indirect_No_Branching()
        {
            var target = new TwoProperties {Foo = "Bar", Bar = "Baz"};
            var bar = _getFoo.Invoke(target);
            var baz = _getBar.Invoke(target);
        }

        [Benchmark(Baseline = true)]
        public void Contrived_Direct_No_Branching()
        {
            var target = new TwoProperties {Foo = "Bar", Bar = "Baz"};
            var bar = DirectAnonymousReadAccessor.Foo(target);
            var baz = DirectAnonymousReadAccessor.Bar(target);
        }

        [Benchmark(Baseline = false)]
        public void DotLiquid_Hash_FromAnonymousObject()
        {
            var target = new {Foo = "Bar", Bar = "Baz"};
            var hash = Hash.FromAnonymousObject(target);
            var bar = hash["Foo"];
            var baz = hash["Bar"];
        }
    }
}