using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Phlatware.Tests
{
    [TestClass]
    public class PhlatTests_Merge
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

        private Foo _targetFoo;
        private Foo _sourceFoo;

        [TestInitialize]
        public void Initting()
        {
            _targetFoo = new Foo
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

            _sourceFoo = new Foo
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
                .HasOne(m => m.Bar, (s,t)=>s.Name == "DELETE ME")       //a little hacky, but we're saying if we name the Bar with "DELETE ME" 'ShouldDelete' will return true.  Used to test deleting. 
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
        public void TestMerge()
        {
            var result = _target.Merge(_sourceFoo, _targetFoo);

            Console.WriteLine(result.ToString());
        }

        [TestMethod]
        public void TestMerge_SinglePropAdded()
        {
            _targetFoo.Bar = null;

            var result = _target.Merge(_sourceFoo, _targetFoo);

            Console.WriteLine(result);
            Assert.AreEqual("Bar be thy name", ((Bar)result[1].Model).Name);
        }

        [TestMethod]
        public void TestMerge_SingleProp()
        {
            _sourceFoo.Bar.Name = "A new name for bar...Cheers?";

            var result = _target.Merge(_sourceFoo, _targetFoo);

            Console.WriteLine(result);
            Assert.AreEqual("A new name for bar...Cheers?",((Bar)result[1].Model).Name);
        }

        [TestMethod]
        public void TestMerge_Deep()
        {
            _sourceFoo.Bazzes[1].Name = "Baz-a WAS thy name...from henceforth you shall be known as Bazzle.";

            var results = _target.Merge(_sourceFoo, _targetFoo);
            Console.WriteLine(results.ToString());

            var result = results.Single(r => r.Model == _targetFoo.Bazzes[1]);
            Assert.AreEqual(ResultStates.Updated,result.State);
            Assert.AreEqual("Baz-a WAS thy name...from henceforth you shall be known as Bazzle.",((Baz)result.Model).Name);
        }

        [TestMethod]
        public void TestMerge_DeepInsert()
        {
            var newBaz = new Baz { Name = "The new Baz on the block." };

            _sourceFoo.Bazzes.Add(newBaz);

            var results = _target.Merge(_sourceFoo, _targetFoo);
            Console.WriteLine(results);

            var newResult = results.Where(w => Object.ReferenceEquals(w.Model, newBaz)).Single();
            Assert.AreEqual(ResultStates.Created,newResult.State);
        }

        [TestMethod]
        public void TestMerge_DeleteRemoved()
        {
            //remove one from our target.  foo2 will have an extra one that should be marked as deleted.
            _sourceFoo.Bazzes.RemoveAt(0);

            var result = _target.Merge(_sourceFoo, _targetFoo);

            Console.WriteLine(result.ToString());

            Assert.AreEqual(ResultStates.Deleted,result[3].State);
        }

        [TestMethod]
        public void TestMerge_DeleteFromListRemovesItemFromTarget()
        {
            //change the default from case-insensitive to case-sensitive
            var target = new Phlat(_config);

            //remove an item from _foo2 and it should be removed from _foo1.
            _sourceFoo.Bazzes.RemoveAt(0);

            var results = _target.Merge(_sourceFoo, _targetFoo);

            Console.Write(results);

            Assert.IsFalse(_targetFoo.Bazzes.Where(baz => baz.Id == 3).Any());
        }

        [TestMethod]
        public void TestMerge_DeletePropertySetsToDefaultOnTarget()
        {
            //change the default from case-insensitive to case-sensitive
            var target = new Phlat(_config);

            //remove an item from _foo2 and it should be removed from _foo1.
            _sourceFoo.Bar.Name = "DELETE ME";

            var results = _target.Merge(_sourceFoo, _targetFoo);

            Console.Write(results);

            Assert.IsNull(_targetFoo.Bar);
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
        public void TestConfiguration_ValueDictionaryIsInvariantCultureIgnoreCase()
        {
            //change the default from case-insensitive to case-sensitive
            var target = new Phlat(_config);

            var results = target.Flatten(_targetFoo);

            Assert.IsTrue(results.RootResult.Values.ContainsKey("id"));
        }

        
    }
}
