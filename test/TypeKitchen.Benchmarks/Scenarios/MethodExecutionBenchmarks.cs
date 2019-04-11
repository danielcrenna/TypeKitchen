using System.Reflection;
using BenchmarkDotNet.Attributes;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(false, true)]
    [CsvMeasurementsExporter]
    [RPlotExporter]
    public class MethodExecutionBenchmarks
    {
        private ObjectMethodExecutor.ObjectMethodExecutor _executor;
        private ClassWithTwoMethodsAndProperty _target;
        private object[] _noArgs;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _target = new ClassWithTwoMethodsAndProperty();
            _executor = ObjectMethodExecutor.ObjectMethodExecutor.Create
            (
                typeof(ClassWithTwoMethodsAndProperty).GetMethod(nameof(ClassWithTwoMethodsAndProperty.Foo)),
                typeof(ClassWithTwoMethodsAndProperty).GetTypeInfo()
            );
            _noArgs = new object[] { };
        }

        [Benchmark(Baseline = false)]
        public void ObjectMethodExecutor_NoArgs()
        {
            _executor.Execute(_target, _noArgs);
        }

        [Benchmark(Baseline = true)]
        public void Native_NoArgs()
        {
            _target.Foo(); // "The method duration is indistinguishable from the empty method duration" :-(
        }
        
    }
}
