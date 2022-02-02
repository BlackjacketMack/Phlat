using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Phlatware.Tests
{
    [TestClass]
    public class Phlat_Types_Tests
    {
        public class Foo
        {
            public int BarInt { get; set; }
            public int? BarIntNullable { get; set; }

            public TimeSpan BarTimeSpan { get; set; }
            public TimeSpan? BarTimeSpanNullable { get; set; }
        }

        private Phlat _target;
        private PhlatConfiguration _config;
        private Foo _foo1;

        [TestInitialize]
        public void Initting()
        {
            _foo1 = new Foo
            {
                BarInt = 0,
                BarIntNullable = 1,
                BarTimeSpan = TimeSpan.FromSeconds(1000),
                BarTimeSpanNullable = TimeSpan.FromSeconds(2000)
            };

            _config = new PhlatConfiguration();

            _config.Configure<Foo>();

            _target = new Phlat(_config);
        }


        [TestMethod]
        public void TestFlatten()
        {
            var result = _target.Flatten(_foo1);

            Console.Write(result);
        }
    }
}
