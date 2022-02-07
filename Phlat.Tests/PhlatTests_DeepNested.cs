using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Phlatware.Tests
{
    [TestClass]
    public class PhlatTests_DeepNested
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

            public override string ToString() => $"{this.GetType().Name}::{Id}::{Name}";
        }

        public class Foo : Base
        {
            public IList<Bar> Bars { get; set; }
        }

        public class Bar : Base
        {
            public IList<Baz> Bazs { get; set; }
        }

        public class Baz : Base
        {
        }

        private Foo _foo1;
        private Foo _sourceFoo;

        [TestInitialize]
        public void Initting()
        {
            _foo1 = new Foo
            {
                Id = 1,
                Name = "Foo be thy name",
                Bars = new List<Bar>
                {
                    new Bar { 
                        Id = 2, 
                        Name = "Bar be thy name",
                        Bazs = new List<Baz>
                        {
                            new Baz{
                                Id = 2,
                                Name = "Baz be thy name"
                            }
                         }
                    }
                }
            };

            _sourceFoo = new Foo
            {
                Id = 1,
                Name = "Foo be thy name",
                Bars = new List<Bar>
                {
                    new Bar {
                        Id = 2,
                        Name = "Bar be thy name",
                        Bazs = new List<Baz>
                        {
                            new Baz{
                                Id = 2,
                                Name = "Baz be thy name"
                            }
                         }
                    }
                }
            };

            _config = new PhlatConfiguration();

            _config.Configure<Foo>((s, t) =>
            {
                t.Name = s.Name;
            })
            .HasMany(m => m.Bars, (s, t) => s == null);

            _config.Configure<Bar>((s, t) =>
            {
                t.Name = s.Name;
            })
            .HasMany(m=>m.Bazs, (s,t)=> s == null);

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
        public void TestMerge_ModifyABaz()
        {
            _sourceFoo.Bars[0].Bazs[0].Name = "A new baz name";

            var results = _target.Merge(_sourceFoo, _foo1);

            Console.WriteLine(results.ToString());

            Assert.AreEqual("A new baz name", _foo1.Bars[0].Bazs[0].Name);
        }

        [TestMethod]
        public void TestMerge_RemoveABaz()
        {
            _sourceFoo.Bars[0].Bazs.RemoveAt(0);

            var bazRemoved = _foo1.Bars[0].Bazs[0];

            var results = _target.Merge(_sourceFoo, _foo1);

            Console.WriteLine(results.ToString());

            var result = results.Where(w => w.Model == bazRemoved).Single();
            Assert.AreEqual(ResultStates.Deleted,result.State);
        }

        [TestMethod]
        public void TestMerge_AddABaz()
        {
            var shinyNewBaz = new Baz { Name = "A shiny new baz" };
            _sourceFoo.Bars[0].Bazs.Add(shinyNewBaz);

            var results = _target.Merge(_sourceFoo, _foo1);

            Console.WriteLine(results.ToString());

            var result = results.Where(w => w.Model == shinyNewBaz).Single();
            Assert.AreEqual(ResultStates.Created, result.State);
        }

        [TestMethod]
        public void TestMerge_AddABarThenABaz()
        {
            var shinyNewBar = new Bar { Name = "A shiny new bar" };
            var shinyNewBaz = new Baz { Name = "A shiny new baz" };
            shinyNewBar.Bazs = new List<Baz> { shinyNewBaz };
            
            _sourceFoo.Bars.Add(shinyNewBar);

            var results = _target.Merge(_sourceFoo, _foo1);

            Console.WriteLine(results.ToString());

            //we want to make sure that baz wasn't added twice
            //bar would be added (it's new) and bar already has that baz
            Assert.AreEqual(1, shinyNewBar.Bazs.Count());

            var result = results.Where(w => w.Model == shinyNewBaz).Single();
            Assert.AreEqual(ResultStates.Created, result.State);

        }
    }
}
