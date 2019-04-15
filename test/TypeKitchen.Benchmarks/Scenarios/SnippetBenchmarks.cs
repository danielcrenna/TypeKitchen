using System;
using System.Reflection;
using System.Reflection.Emit;
using BenchmarkDotNet.Attributes;

namespace TypeKitchen.Benchmarks.Scenarios
{
    [CoreJob]
    [MemoryDiagnoser]
    [DisassemblyDiagnoser(false, true)]
    [CsvMeasurementsExporter]
    [RPlotExporter]
    public class SnippetBenchmarks
    {
        private IMethodCallAccessor _wrapped;
        private MethodInfo _method;
        private MethodInfo _invoke;
        private Func<int> _emit;
        private Func<object, object[], object> _lateBound;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _method = Snippet.CreateMethod("public static int Method() { return 1; }");
            _wrapped = CallAccessor.Create(_method);
            _invoke = typeof(SnippetBenchmarks).GetMethod("Method");
            _emit = CreateAnonymousMethod();
            _lateBound = LateBinding.DynamicMethodBindCall(_method);
        }
        
        [Benchmark(Baseline = false)]
        public void Invoke_Snippet()
        {
            _method.Invoke(null, null);
        }

        [Benchmark(Baseline = false)]
        public void Wrapped_Snippet()
        {
            _wrapped.Call(null, null);
        }

        [Benchmark(Baseline = false)]
        public void Invoke_MethodInfo()
        {
            _invoke.Invoke(null, null);
        }

        [Benchmark(Baseline = false)]
        public void Invoke_LateBound_DynamicMethod()
        {
            _lateBound.Invoke(null, null);
        }

        [Benchmark(Baseline = true)]
        public void Emit_Internal()
        {
            _emit();
        }

        public static int Method() { return 1; }

        private static Func<int> CreateAnonymousMethod()
        {
            var dm = new DynamicMethod($"__", typeof(int), Type.EmptyTypes);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
            return (Func<int>)dm.CreateDelegate(typeof(Func<int>));
        }
    }
}
