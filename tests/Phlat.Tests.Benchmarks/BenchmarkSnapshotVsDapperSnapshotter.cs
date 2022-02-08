using BenchmarkDotNet.Attributes;
using Dapper;
using Phlatware;
using System.Collections.Generic;
using System.Text;

namespace Phlat.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkSnapshotVsDapperSnapshotter
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

        private Foo _foo;


        public BenchmarkSnapshotVsDapperSnapshotter()
        {
            _foo = new Foo();

        }

        [Benchmark]
        public void TestPhlatSnapshot()
        {
            for(var i = 0;i < NumberOfItems; i++)
            {
                var target = new Snapshot<Foo>(_foo);

                target.Values();

                mutateFoo();

                var changes = target.Values();
            }
        }

        [Benchmark]
        public void TestDapperSnapshotter()
        {
            for (var i = 0; i < NumberOfItems; i++)
            {
                var target = Snapshotter.Start(_foo);

                mutateFoo();

                var changes = target.Diff();
            }
        }

        private void mutateFoo()
        {
            _foo.Name = "Changed value";
        }
    }
}
