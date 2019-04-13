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
    public class CallAccessorBenchmarks
    {
        private ObjectMethodExecutor.ObjectMethodExecutor _executor;
        private ClassWithTwoMethodsAndProperty _target;
        private object[] _noArgs;
        private IMethodCallAccessor _direct;
        private IMethodCallAccessor _accessor;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var voidNoArgsMethod = typeof(ClassWithTwoMethodsAndProperty).GetMethod(nameof(ClassWithTwoMethodsAndProperty.Foo));

            _target = new ClassWithTwoMethodsAndProperty();
            _executor = ObjectMethodExecutor.ObjectMethodExecutor.Create
            (
                voidNoArgsMethod,
                typeof(ClassWithTwoMethodsAndProperty).GetTypeInfo()
            );
            _direct = new DirectCallAccessor();
            _noArgs = new object[] { };
            _accessor = CallAccessor.Create(voidNoArgsMethod);
        }

        [Benchmark(Baseline = false)]
        public void ObjectMethodExecutor_Void_NoArgs()
        {
            _executor.Execute(_target, _noArgs);
        }

        [Benchmark(Baseline = false)]
        public void CallAccessor_Void_NoArgs()
        {
            _accessor.Call(_target);
        }

        [Benchmark(Baseline = true)]
        public void Direct_Void_NoArgs()
        {
            _direct.Call(_target);
        }
    }
}
