// Copyright (c) Blowdart, Inc. All rights reserved.

using BenchmarkDotNet.Running;
using TypeKitchen.Benchmarks.Scenarios;

namespace TypeKitchen.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<AnonymousTypeBenchmarks>();
            BenchmarkRunner.Run<ReadAccessorBenchmarks>();
            BenchmarkRunner.Run<WriteAccessorBenchmarks>();
        }
    }
}