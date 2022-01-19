using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Phlatware.Tests
{
    [TestClass]
    public class SnapshotT_UsingEmitTests
    {
        public class Foo
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestValues()
        {
            var foo = new Foo
            {
                Id = 1,
                Name = "Jake Peralta"
            };

            var snapshot = new SnapshotT_UsingEmit<Foo>(foo);
            var dict = snapshot.Values();

            Assert.AreEqual(1, dict[nameof(foo.Id)]);
            Assert.AreEqual("Jake Peralta", dict[nameof(foo.Name)]);
        }

        [TestMethod]
        public void TestChanges()
        {
            var foo = new Foo
            {
                Id = 1,
                Name = "Jake Peralta"
            };

            var snapshot = new SnapshotT_UsingEmit<Foo>(foo);
            snapshot.Start();

            foo.Name = "Terry";

            var changes = snapshot.Changes();

            Assert.AreEqual("Terry", changes[nameof(foo.Name)]);
        }
    }
}
