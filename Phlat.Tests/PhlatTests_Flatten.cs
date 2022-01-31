using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Phlatware.Tests
{
    [TestClass]
    public class PhlatTests_Flatten
    {
        private Phlat _target;
        private PhlatConfiguration _config;

        public class Base
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                return this.Id == ((Base)obj).Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        public class Foo : Base
        {
            public Bar Bar { get; set; }
            public IList<Baz> Bazzes { get; set; }
        }

        public class Bar : Base
        {
        }

        public class Baz : Base
        {
        }

        private Foo _foo1;
        private Foo _foo2;

        [TestInitialize]
        public void Initting()
        {
            _foo1 = new Foo
            {
                Id = 1,
                Name = "Foo be thy name",
                Bar = new Bar { Id = 2, Name = "Bar be thy name" },
                Bazzes = new List<Baz>
                {
                    new Baz{Id = 3, Name = "Baz-a be thy name"},
                    new Baz{Id = 4, Name = "Baz-b be thy name"},
                }
            };

            _config = new PhlatConfiguration();

            _config.Configure<Foo>((s, t) =>
            {
                t.Name = s.Name;
            })
                .HasOne(m => m.Bar)
                .HasMany(m => m.Bazzes, (s, t) => s == null);

            _config.Configure<Bar>((s, t) =>
            {
                t.Name = s.Name;
            });

            _config.Configure<Baz>((s, t) =>
            {
                t.Name = s.Name;
            });


            _target = new Phlat(_config);
        }

        [TestMethod]
        public void TestFlatten()
        {
            var result = _target.Flatten(_foo1);

            Console.WriteLine(result.ToString());

            Assert.IsTrue(result.TrueForAll(r => r.Values != null));
        }

        [TestMethod]
        public void TestFlatten_WithValues()
        {
            var result = _target.Flatten(_foo1);

            Console.WriteLine(result.ToString());

            Assert.IsTrue(result.TrueForAll(r => r.Values != null));
        }

        [TestMethod]
        public void TestFlatten_SingleProperty()
        {
            var results = _target.Flatten(_foo1);

            Console.WriteLine(results.ToString());

            Assert.IsTrue(results.Any(w => w.Model == _foo1.Bar));
        }

        [TestMethod]
        public void TestFlatten_ValueDictionaryIsInvariantCultureIgnoreCase()
        {
            var target = new Phlat(_config);

            var results = target.Flatten(_foo1);

            Assert.IsTrue(results.RootResult.Values.ContainsKey("id"));
        }
    }
}
