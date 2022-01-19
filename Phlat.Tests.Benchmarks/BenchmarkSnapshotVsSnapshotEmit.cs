using BenchmarkDotNet.Attributes;
using Dapper;
using Phlatware;
using System;
using System.Collections.Generic;
using System.Text;

namespace Phlat.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkSnapshotVsSnapshotEmit
    {
        int NumberOfItems = 100000;

        public class Foo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Prop1 { get; set; }
        }

        private Foo getFoo()
        {
            return new Foo
            {
                Id = 9,
                Name = "Boba Fett"
            };
        }

        public BenchmarkSnapshotVsSnapshotEmit()
        {

        }

        [Benchmark]
        public void TestPhlatSnapshot()
        {
            runTest(foo => new Snapshot<Foo>(foo));
        }

        [Benchmark]
        public void TestSnapshotter3()
        {
            runTest(foo=> new SnapshotT_UsingEmit<Foo>(foo));
        }

        private void runTest(Func<Foo,ISnapshot> snapshot)
        {
            for (var i = 0; i < NumberOfItems; i++)
            {
                var foo = getFoo();

                var target = snapshot(foo);

                target.Start();

                mutateFoo(foo);

                target.Compare();
            }
        }

        private void mutateFoo(Foo foo)
        {
            foo.Name = "Changed value";
        }
    }
}
