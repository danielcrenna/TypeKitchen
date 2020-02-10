// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TypeKitchen.Creation;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
	public delegate object CreateInstanceNoParams();

	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	[MemoryDiagnoser]
	[DisassemblyDiagnoser(false, true)]
	[CsvMeasurementsExporter]
	[RPlotExporter]
	public class ActivationBenchmarks
	{
		private CreateInstance _activatorArgs;
		private CreateInstance _activatorNoArgs;
		private CreateInstance _dynamicMethodArgs;

		private CreateInstance _dynamicMethodNoArgs;
		private CreateInstanceNoParams _dynamicMethodNoArgsExplicit;
		private CreateInstance _expressionArgs;

		private CreateInstance _expressionNoArgs;
		private CreateInstanceNoParams _expressionNoArgsExplicit;
		private CreateInstance _invokeArgs;
		private CreateInstance _invokeNoArgs;

		[GlobalSetup]
		public void GlobalSetup()
		{
			var ctorNoArgs = typeof(ClassWithTwoMethodsAndProperty).GetConstructor(Type.EmptyTypes);
			var ctorArgs = typeof(ClassWithTwoMethodsAndProperty).GetConstructor(new[] {typeof(int)});

			_activatorNoArgs = Activation.ActivatorWeakTyped(ctorNoArgs);
			_activatorArgs = Activation.ActivatorWeakTyped(ctorArgs);

			_invokeNoArgs = Activation.InvokeWeakTyped(ctorNoArgs);
			_invokeArgs = Activation.InvokeWeakTyped(ctorArgs);

			_dynamicMethodNoArgs = Activation.DynamicMethodWeakTyped(ctorNoArgs);
			_dynamicMethodNoArgsExplicit = DynamicMethodWeakTypedNoParams(ctorNoArgs);
			_dynamicMethodArgs = Activation.DynamicMethodWeakTyped(ctorArgs);

			_expressionNoArgs = Activation.ExpressionWeakTyped(ctorNoArgs);
			_expressionNoArgsExplicit = ExpressionWeakTypedNoParams(ctorNoArgs);
			_expressionArgs = Activation.ExpressionWeakTyped(ctorArgs);
		}

		[Benchmark(Baseline = true)]
		public void Activator_WeakTyped_CreateInstance_NoParams()
		{
			var foo = _activatorNoArgs();
		}

		[Benchmark(Baseline = false)]
		public void Activator_WeakTyped_CreateInstance_Params()
		{
			var foo = _activatorArgs(100);
		}

		[Benchmark(Baseline = false)]
		public void Activation_WeakTyped_Invoke_NoParams()
		{
			var foo = _invokeNoArgs();
		}

		[Benchmark(Baseline = false)]
		public void Activation_WeakTyped_Invoke_Params()
		{
			var foo = _invokeArgs(100);
		}

		[Benchmark(Baseline = false)]
		public void Activation_WeakTyped_DynamicMethod_NoParams()
		{
			var foo = _dynamicMethodNoArgs();
		}

		[Benchmark(Baseline = false)]
		public void Activation_WeakTyped_DynamicMethod_Params()
		{
			var foo = _dynamicMethodArgs(100);
		}

		[Benchmark(Baseline = false)]
		public void Activation_WeakTyped_Expression_NoParams()
		{
			var foo = _expressionNoArgs();
		}

		[Benchmark(Baseline = false)]
		public void Activation_WeakTyped_Expression_Params()
		{
			var foo = _expressionArgs(100);
		}

		#region Failed Experiments

		[Benchmark(Baseline = false)] // a custom delegate for no parameters doesn't impact results
		public void Activation_DynamicMethod_WeakTyped_NoParams_Explicit_Delegate()
		{
			var foo = _dynamicMethodNoArgsExplicit();
		}

		[Benchmark(Baseline = false)] // a custom delegate for no parameters doesn't impact results
		public void Activation_Expression_WeakTyped_NoParams_Explicit_Delegate()
		{
			var foo = _expressionNoArgsExplicit();
		}

		public static CreateInstanceNoParams DynamicMethodWeakTypedNoParams(ConstructorInfo ctor)
		{
			var dm = new DynamicMethod($"Construct_{ctor.MetadataToken}", ctor.DeclaringType, Type.EmptyTypes, true);
			var il = dm.GetILGenerator();
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Ret);
			return (CreateInstanceNoParams) dm.CreateDelegate(typeof(CreateInstanceNoParams));
		}

		public static CreateInstanceNoParams ExpressionWeakTypedNoParams(ConstructorInfo ctor)
		{
			var lambda = Expression.Lambda(typeof(CreateInstanceNoParams), Expression.New(ctor));
			var compiled = (CreateInstanceNoParams) lambda.Compile();
			return compiled;
		}

		#endregion
	}
}