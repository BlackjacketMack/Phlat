using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Phlatware.Tests
{
    [TestClass]
    public class PhlatTypeExtensionsTests
    {
        private PhlatConfiguration _config;

        public class Base
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsDeleted { get; set; }

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
            public IList<Baz> Bazzes { get; set; }
        }

        public class Baz : Base
        {
        }

        private Foo _targetFoo;
        private Foo _sourceFoo;

        [TestInitialize]
        public void Initting()
        {
            _sourceFoo = new Foo
            {
                Id = 1,
                Name = "Foo be thy new name",
                Description = "A new description",
                Bazzes = new List<Baz>
                {
                    new Baz{Id = 3, Name = "Baz-a be thy name"},
                    new Baz{Id = 4, Name = "Baz-b be thy name"},
                }
            };

            _targetFoo = new Foo
            {
                Id = 1,
                Name = "Foo be thy name",
                Bazzes = new List<Baz>
                {
                    new Baz{Id = 3, Name = "Baz-a be thy name"},
                    new Baz{Id = 4, Name = "Baz-b be thy name"},
                }
            };

            //basic configuration.  individual tests will configure paths.
            _config = new PhlatConfiguration();

            _config.Configure<Foo>((s, t) =>
            {
                t.Name = s.Name;
            });

            _config.Configure<Baz>((s, t) =>
            {
                t.Name = s.Name;
            });
        }

        [TestMethod]
        public void TestHasMany_DeleteIfMissing()
        {
            var firstBaz = _sourceFoo.Bazzes[0];
            _sourceFoo.Bazzes.Remove(firstBaz);

            var phlatType = _config.GetPhlatType<Foo>()
                                .HasMany(m => m.Bazzes, deleteIfSourceMissing:true);

            var phlat = new Phlat(_config);

            var results = phlat.Merge(_sourceFoo, _targetFoo);

            Assert.IsFalse(_targetFoo.Bazzes.Contains(firstBaz));
        }

        [TestMethod]
        public void TestHasMany_DeleteIfSourceHas()
        {
            var firstBaz = _sourceFoo.Bazzes[0];
            firstBaz.IsDeleted = true;

            var phlatType = _config.GetPhlatType<Foo>()
                                .HasMany(m => m.Bazzes, deleteIfSourceHas: s=>s.IsDeleted);

            var phlat = new Phlat(_config);

            var results = phlat.Merge(_sourceFoo, _targetFoo);

            Assert.IsFalse(_targetFoo.Bazzes.Contains(firstBaz));
        }
    }
}
