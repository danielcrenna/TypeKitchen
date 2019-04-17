// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Running;
using TypeKitchen.Benchmarks.Micro;
using TypeKitchen.Benchmarks.Scenarios;

namespace TypeKitchen.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //BenchmarkRunner.Run<AnonymousTypeBenchmarks>();
            //BenchmarkRunner.Run<ReadAccessorBenchmarks>();
            //BenchmarkRunner.Run<WriteAccessorBenchmarks>();
            //BenchmarkRunner.Run<SingletonTypeResolverBenchmarks>();
            //BenchmarkRunner.Run<DuckCastingBenchmarks>();
            //BenchmarkRunner.Run<CallAccessorBenchmarks>();
            //BenchmarkRunner.Run<ActivationBenchmarks>();
            //BenchmarkRunner.Run<SnippetBenchmarks>();
            BenchmarkRunner.Run<DictionaryAccessBenchmarks>();
        }
    }
}
