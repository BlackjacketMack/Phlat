using System.Collections.Generic;

namespace Phlatware.Tests.EF
{
    public class EFSetup
    {
        /// <summary>
        /// Creates dummy data and inserts it inot our BloggingContext
        /// </summary>
        public static void LoadUpEF()
        {
            var blog = createBlog();

            using (var dbContext = new BloggingContext())
            {
                dbContext.Blogs.AddRange(new[] { blog });

                dbContext.SaveChanges();
            }
        }

        private static Blog createBlog()
        {
            return new Blog
            {
                Id = 1,
                Rating = 1,
                Url = "url1",
                Posts = new List<Post>
                    {
                        new Post
                        {
                            Id = 11, Content = "content1", Title = "title1", BlogId = 1
                        },
                        new Post
                        {
                            Id = 12, Content = "New Post", Title = "I'm a new post", BlogId = 1
                        }
                    }
            };
        }
    }
    
}
