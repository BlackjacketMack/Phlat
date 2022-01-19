using BenchmarkDotNet.Attributes;
using Phlatware;
using System.Collections.Generic;
using System.Text;

namespace Phlat.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkSnapshot
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

        private ISnapshot _target;

        public BenchmarkSnapshot()
        {
            _foo = new Foo();

        }

        [Benchmark]
        public void TestSnapshot()
        {
            for(var i = 0;i < NumberOfItems; i++)
            {
                _target = new Snapshot<Foo>(_foo);

                _target.Start();

                _foo.Name = "Changed value";

                _target.Changes();
            }
        }
    }
}
