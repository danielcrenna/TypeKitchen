// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace TypeKitchen.Benchmarks.Micro
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	[MemoryDiagnoser]
	[DisassemblyDiagnoser(false, true)]
	[CsvMeasurementsExporter]
	[RPlotExporter]
	public class DictionaryAccessBenchmarks
	{
		private readonly Dictionary<int, ITypeReadAccessor> _byMetadataToken = new Dictionary<int, ITypeReadAccessor>();
		private readonly Dictionary<Type, ITypeReadAccessor> _byType = new Dictionary<Type, ITypeReadAccessor>();

		[GlobalSetup]
		public void SetUp()
		{
			var instance = new {Foo = "Bar", Bar = "Baz"};
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
			return new {Foo = "Bar", Bar = "Baz"};
		}
	}
}