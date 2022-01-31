using BenchmarkDotNet.Attributes;
using Dapper;
using Phlatware;
using System;
using System.Collections.Generic;
using System.Text;

namespace Phlat.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkSnapshotVsSnapshotter2VsDapperSnapshotter_Changes
    {
        int NumberOfItems = 100000;

        public class Foo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            public string Prop3 { get; set; }
            public string Prop4 { get; set; }
            public string Prop5 { get; set; }
            public string Prop6 { get; set; }
            public string Prop7 { get; set; }
            public string Prop8 { get; set; }
        }

        private Foo getFoo()
        {
            return new Foo
            {
                Id = 9,
                Name = "Boba Fett"
            };
        }

        public BenchmarkSnapshotVsSnapshotter2VsDapperSnapshotter_Changes()
        {

        }

        [Benchmark]
        public void TestSnapshot()
        {
            runTest(foo => new Snapshot<Foo>(foo));
        }

        [Benchmark]
        public void TestSnapshotEmit()
        {
            runTest(foo=> new SnapshotT_UsingEmit<Foo>(foo));
        }

        [Benchmark]
        public void TestDapperSnapshotter()
        {
            runTest(foo => new DapperSnapshotterWrapper<Foo>(foo));
        }

        private void runTest(Func<Foo,ISnapshot> snapshot)
        {
            for (var i = 0; i < NumberOfItems; i++)
            {
                var foo = getFoo();

                var target = snapshot(foo);

                target.Values();
            }
        }

        private void mutateFoo(Foo foo)
        {
            foo.Name = "Terry";
        }
    }
}
