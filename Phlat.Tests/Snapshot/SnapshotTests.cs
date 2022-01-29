using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Phlatware.Tests
{
    [TestClass]
    public class SnapshotTests
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

            var snapshot = new Snapshot<Foo>(foo);
            var dict = snapshot.Values();

            Assert.AreEqual(1, dict[nameof(foo.Id)]);
            Assert.AreEqual("Jake Peralta", dict[nameof(foo.Name)]);
        }

        [TestMethod]
        public void TestCompare()
        {
            var foo = new Foo
            {
                Id = 1,
                Name = "Jake Peralta"
            };

            var snapshot = new Snapshot<Foo>(foo);
            var startValues = snapshot.Start();

            foo.Name = "Terry Jeffords";

            var changes = snapshot.Compare();

            Assert.AreEqual(1, changes[nameof(foo.Id)].NewValue);
            Assert.AreEqual("Jake Peralta", changes[nameof(foo.Name)].OldValue);
            Assert.AreEqual("Terry Jeffords", changes[nameof(foo.Name)].NewValue);
        }

        [TestMethod]
        public void TestChanges()
        {
            var foo = new Foo
            {
                Id = 1,
                Name = "Jake Peralta"
            };

            var snapshot = new Snapshot<Foo>(foo);
            snapshot.Start();
            foo.Name = "Terry Jeffords";

            var changes = snapshot.Changes();

            Assert.IsFalse(changes.ContainsKey(nameof(Foo.Id)));
            Assert.AreEqual("Terry Jeffords", changes[nameof(foo.Name)]);
        }

        [TestMethod]
        public void TestSnapshotOptions_DictionaryStringComparer_DefaultInvariantCultureIgnoreCase()
        {
            var foo = new Foo
            {
                Id = 1,
                Name = "Jake Peralta"
            };

            var snapshot = new Snapshot<Foo>(foo);
            var values = snapshot.Values();

            Assert.IsTrue(values.ContainsKey("id"));
        }
    }
}
