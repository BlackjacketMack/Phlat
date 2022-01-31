using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Phlatware.Tests.EF
{
    [TestClass]
    public class EFTests
    {
        private Phlat _phlat;

        [TestInitialize]
        public void Init()
        {
            _phlat = PhlatSetup.ConfigurePhlat();

            EFSetup.LoadUpEF();
        }

        private Blog getBlogFromDbContext(BloggingContext dbContext)
        {
            return dbContext.Blogs.Where(b => b.Id == 1)
                                        .Include(b => b.Posts)
                                        .AsTracking()
                                        .Single();

        }

        /// <summary>
        /// We're going to pass in json to emulate data coming from the ui tier.
        /// This could also be data from a viewmodel.
        /// </summary>
        /// <returns></returns>
        private string blogJsonFromAnApi()
        {
            return @"
                        {
                            ""Id"":1,
                            ""Url"":""Some url changd by the ui."",
                            ""Posts"":[
                                    {
                                        ""Id"":11,
                                        ""Title"":""Title for 11"",
                                        ""Content"":""Content for 11""
                                    },

                                    {
                                        ""Title"":""New one here!"",
                                        ""Content"":""This item is being added by the ui.""
                                    }
                                ]
                        }
                    ";
        }

        [TestMethod]
        public void TestSetup()
        {
            using(var dbContext = new BloggingContext())
            {
                var blog = getBlogFromDbContext(dbContext);

                Assert.IsNotNull(blog);
                Assert.AreEqual(2,blog.Posts.Count);
            }
        }

        [TestMethod]
        public void TestMerge()
        {
            var sourceBlog = JsonSerializer.Deserialize<Blog>(blogJsonFromAnApi());

            using (var dbContext = new BloggingContext())
            {
                var targetBlog = getBlogFromDbContext(dbContext);               //this example should have change tracking enabled

                var results = _phlat.Merge(sourceBlog, targetBlog);

                //get deleted items and set them as deleted in ef
                foreach(var result in results.Where(w => w.State == ResultStates.Deleted))
                {
                    dbContext.Entry(result.Model).State = EntityState.Deleted;
                }

                dbContext.SaveChanges();

                targetBlog = getBlogFromDbContext(dbContext);

                //the root Blog had a property changed
                Assert.AreEqual("Some url changd by the ui.", targetBlog.Url);

                //the second post should have been deleted
                Assert.IsFalse(targetBlog.Posts.Where(w=>w.Id == 12).Any());

                //the new item was added to the collection
                Assert.AreEqual("New one here!", targetBlog.Posts.Last().Title);
            }
        }
    }
}
