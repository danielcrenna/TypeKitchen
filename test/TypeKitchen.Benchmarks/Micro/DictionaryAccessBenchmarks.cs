using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace TypeKitchen.Benchmarks.Micro
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(false, true)]
    [CsvMeasurementsExporter]
    [RPlotExporter]
    public class DictionaryAccessBenchmarks
    {
        private readonly Dictionary<Type, ITypeReadAccessor> _byType = new Dictionary<Type, ITypeReadAccessor>();
        private readonly Dictionary<int, ITypeReadAccessor> _byMetadataToken = new Dictionary<int, ITypeReadAccessor>();
        
        [GlobalSetup]
        public void SetUp()
        {
            var instance = new { Foo = "Bar", Bar = "Baz" };
            var type = instance.GetType();

            var accessor = ReadAccessor.Create(type);
            _byType.Add(type, accessor);
            _byMetadataToken.Add(type.MetadataToken, accessor);
        }

        [Benchmark]
        public void ByType()
        {
            var target = GetOutOfMethodTarget();
            var type = target.GetType();
            var foo = _byType[type][target, "Foo"];
        }

        [Benchmark]
        public void ByMetadataToken()
        {
            var target = GetOutOfMethodTarget();
            var type = target.GetType();
            var foo = _byMetadataToken[type.MetadataToken][target, "Foo"];
        }
        
        public object GetOutOfMethodTarget()
        {
            return new { Foo = "Bar", Bar = "Baz" };
        }
    }
}
