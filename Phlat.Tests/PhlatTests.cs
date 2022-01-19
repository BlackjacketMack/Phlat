using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Phlatware.Tests
{
    [TestClass]
    public class PhlatTests
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

            _foo2 = new Foo
            {
                Id = 1,
                Name = "Foo be thy new name",
                Description = "A new description",
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

            Assert.IsTrue(result.TrueForAll(r => r.Values == null));
        }

        [TestMethod]
        public void TestFlatten_WithValues()
        {
            var result = _target.Flatten(_foo1,includeValues:true);

            Console.WriteLine(result.ToString());

            Assert.IsTrue(result.TrueForAll(r => r.Values != null));
        }

        [TestMethod]
        public void TestCreate()
        {
            var result = _target.Create(_foo1);

            Console.WriteLine(result.ToString());

            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result.TrueForAll(m=>m.State == ResultStates.Created));
        }

        [TestMethod]
        public void TestModify()
        {
            var result = _target.Modify(_foo2, _foo1);

            Console.WriteLine(result.ToString());
        }

        [TestMethod]
        public void TestModify_SingleProp()
        {
            _foo2.Bar.Name = "A new name for bar...Cheers?";

            var result = _target.Modify(_foo2, _foo1);

            Console.WriteLine(result);
            Assert.AreEqual("A new name for bar...Cheers?",((Bar)result[1].Model).Name);
        }

        [TestMethod]
        public void TestModify_Deep()
        {
            _foo2.Bazzes[1].Name = "Baz-a WAS thy name...from henceforth you shall be known as Bazzle.";

            var result = _target.Modify(_foo2, _foo1);

            Console.WriteLine(result.ToString());
        }

        [TestMethod]
        public void TestModify_DeepInsert()
        {
            _foo2.Bazzes.Add(new Baz { Name = "The new Baz on the block." });

            var result = _target.Modify(_foo2, _foo1);

            Console.WriteLine(result.ToString());
        }

        [TestMethod]
        public void TestModify_DeleteRemoved()
        {
            //remove one from our target.  foo2 will have an extra one that should be marked as deleted.
            _foo2.Bazzes.RemoveAt(0);

            var result = _target.Modify(_foo2, _foo1);

            Console.WriteLine(result.ToString());

            Assert.AreEqual(ResultStates.Deleted,result[3].State);
        }

        private class CustomComparerByName : IEqualityComparer<Foo>
        {
            public bool Equals(Foo x, Foo y)
            {
                return x?.Name == y?.Name;
            }

            public int GetHashCode([DisallowNull] Foo obj)
            {
                return obj?.Name?.GetHashCode() ?? default;
            }
        }

        [TestMethod]
        public void TestModify_CustomEqualityByName()
        {
            //_config = new PhlatConfiguration<Foo>()
            //                       .AddRootDefinition((s, t) =>
            //                       {
            //                           t.Description = s.Description;
            //                       },
            //                       comparer: new CustomComparerByName());


            ////remove one from our target.  foo2 will have an extra one that should be marked as deleted.
            //_foo2.Id = 9999; //set id so it wouldn't match
            //_foo2.Name = _foo1.Name;    //these have to match
            //_foo2.Description = "Some new description";

            //var result = Phlat.Modify(_foo2, _foo1, _config);

            //Console.WriteLine(result.ToString());

            //Assert.AreEqual(ResultStates.Updated, result[0].State);
            //Assert.IsTrue(result[0].Changes.ContainsKey(nameof(_foo2.Description)));
        }
    }
}
