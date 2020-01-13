// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Reflection.Emit;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace TypeKitchen.Benchmarks.Scenarios
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	[MemoryDiagnoser]
	[DisassemblyDiagnoser(false, true)]
	[CsvMeasurementsExporter]
	[RPlotExporter]
	public class SnippetBenchmarks
	{
		private IMethodCallAccessor _accessorMethod;
		private IMethodCallAccessor _accessorSnippet;
		private Func<int> _emit;
		private MethodInfo _invoke;
		private Func<object, object[], object> _lateBoundMethod;
		private Func<object, object[], object> _lateBoundSnippet;

		private MethodInfo _snippet;

		[GlobalSetup]
		public void GlobalSetup()
		{
			_snippet = Snippet.CreateMethod("public static int Method() { return 1; }");
			_invoke = typeof(SnippetBenchmarks).GetMethod("Method");
			_accessorSnippet = CallAccessor.Create(_snippet);
			_accessorMethod = CallAccessor.Create(_invoke);
			_emit = CreateAnonymousMethod();
			_lateBoundSnippet = LateBinding.DynamicMethodBindCall(_snippet);
			_lateBoundMethod = LateBinding.DynamicMethodBindCall(_invoke);
		}

		[Benchmark(Baseline = false)]
		public void Invoke_Snippet()
		{
			_snippet.Invoke(null, null);
		}

		[Benchmark(Baseline = false)]
		public void Invoke_Method()
		{
			_invoke.Invoke(null, null);
		}

		[Benchmark(Baseline = false)]
		public void Accessor_Snippet()
		{
			_accessorSnippet.Call(null, null);
		}

		[Benchmark(Baseline = false)]
		public void Accessor_Method()
		{
			_accessorSnippet.Call(null, null);
		}

		[Benchmark(Baseline = false)]
		public void LateBound_Snippet()
		{
			_lateBoundSnippet.Invoke(null, null);
		}

		[Benchmark(Baseline = false)]
		public void LateBound_Method()
		{
			_lateBoundMethod.Invoke(null, null);
		}

		[Benchmark(Baseline = true)]
		public void Emit_Internal()
		{
			_emit();
		}

		public static int Method() { return 1; }

		private static Func<int> CreateAnonymousMethod()
		{
			var dm = new DynamicMethod("__", typeof(int), Type.EmptyTypes);
			var il = dm.GetILGenerator();
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Ret);
			return (Func<int>) dm.CreateDelegate(typeof(Func<int>));
		}
	}
}