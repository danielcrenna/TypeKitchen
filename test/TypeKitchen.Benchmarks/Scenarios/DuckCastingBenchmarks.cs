// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Dynamic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Dynamitey;
using ImpromptuInterface;
using TypeKitchen.Tests.Fakes;

namespace TypeKitchen.Benchmarks.Scenarios
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	[MemoryDiagnoser]
	[DisassemblyDiagnoser(exportDiff: true)]
	[CsvMeasurementsExporter]
	[RPlotExporter]
	public class DuckCastingBenchmarks
	{
		private ITwoMethodsAndProperty _concrete;
		private ITwoMethodsAndProperty _impromptuAnonymous;
		private ITwoMethodsAndProperty _impromptuConcrete;
		private ITwoMethodsAndProperty _impromptuDynamic;
		private DirectDuckCastProxy _proxy;

		[GlobalSetup]
		public void GlobalSetup()
		{
			var count = 0;
			var anon = new
			{
				Foo = ReturnVoid.Arguments(() => count++),
				Bar = ReturnVoid.Arguments<int>(i => count += i),
				Baz = "ABC"
			};
			_impromptuAnonymous = anon.ActLike<ITwoMethodsAndProperty>();

			dynamic expando = new ExpandoObject();
			expando.Foo = ReturnVoid.Arguments(() => count++);
			expando.Bar = ReturnVoid.Arguments<int>(i => count += i);
			expando.Baz = "ABC";
			_impromptuDynamic = Impromptu.ActLike(expando);

			_impromptuConcrete = new ConformingClass().ActLike<ITwoMethodsAndProperty>();

			_concrete = new ClassWithTwoMethodsAndProperty();

			_proxy = new DirectDuckCastProxy(_concrete);
		}

		[Benchmark(Baseline = false)]
		public void ImpromptuInterface_Anonymous_Type()
		{
			_impromptuAnonymous.Foo();
			_impromptuAnonymous.Bar(10);
			var baz = _impromptuAnonymous.Baz;
		}

		[Benchmark(Baseline = false)]
		public void ImpromptuInterface_Dynamic_Type()
		{
			_impromptuDynamic.Foo();
			_impromptuDynamic.Bar(10);
			var baz = _impromptuDynamic.Baz;
		}

		[Benchmark(Baseline = false)]
		public void ImpromptuInterface_Concrete_Type()
		{
			_impromptuConcrete.Foo();
			_impromptuConcrete.Bar(10);
			var baz = _impromptuConcrete.Baz;
		}

		[Benchmark(Baseline = false)]
		public void Contrived_Concrete()
		{
			_proxy.Foo();
			_proxy.Bar(10);
			var baz = _proxy.Baz;
		}

		[Benchmark(Baseline = true)]
		public void No_Duck_Casting()
		{
			_concrete.Foo();
			_concrete.Bar(10);
			var baz = _concrete.Baz;
		}

		public class ConformingClass
		{
			public int Count;

			public string Baz => "ABC";

			public void Foo()
			{
				Count++;
			}

			public void Bar(int i)
			{
				Count += i;
			}
		}
	}
}