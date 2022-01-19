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
            var summary = BenchmarkRunner.Run<BenchmarkSnapshotVsDapperSnapshotter>();
        }
    }
}
