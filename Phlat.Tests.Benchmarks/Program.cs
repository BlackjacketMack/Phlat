using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Text;

namespace Phlat.Tests.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<BenchmarkSnapshot>();
            //var summary = BenchmarkRunner.Run<BenchmarkSnapshotVsDapperSnapshotter>();
            //var summary = BenchmarkRunner.Run<BenchmarkSnapshotVsSnapshotter2>();
            var summary = BenchmarkRunner.Run<BenchmarkSnapshotVsSnapshotter2VsDapperSnapshotter_Changes>();
        }
    }
}
