// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TypeKitchen.Benchmarks.ObjectExecution;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	[MemoryDiagnoser]
	[DisassemblyDiagnoser(false, true)]
	[CsvMeasurementsExporter]
	[RPlotExporter]
	public class CallAccessorBenchmarks
	{
		private IMethodCallAccessor _accessor;
		private IMethodCallAccessor _direct;
		private ObjectMethodExecutor _executor;
		private Func<object, object[], object> _lateBound;
		private object[] _noArgs;
		private ClassWithTwoMethodsAndProperty _target;

		[GlobalSetup]
		public void GlobalSetup()
		{
			var method = typeof(ClassWithTwoMethodsAndProperty).GetMethod(nameof(ClassWithTwoMethodsAndProperty.Foo));

			_target = new ClassWithTwoMethodsAndProperty();
			_executor = ObjectMethodExecutor.Create
			(
				method,
				typeof(ClassWithTwoMethodsAndProperty).GetTypeInfo()
			);
			_direct = new DirectCallAccessor();
			_noArgs = new object[] { };
			_accessor = CallAccessor.Create(method);
			_lateBound = LateBinding.DynamicMethodBindCall(method);
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

		[Benchmark(Baseline = false)]
		public void LateBound_Void_NoArgs()
		{
			_lateBound(_target, null);
		}

		[Benchmark(Baseline = true)]
		public void Direct_Void_NoArgs()
		{
			_direct.Call(_target);
		}
	}
}