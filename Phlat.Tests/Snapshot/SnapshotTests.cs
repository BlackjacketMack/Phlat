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
