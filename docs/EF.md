# Phlat and Entity Framework

Phlat works well with Entity Framework (EF) because it mutates the target during a merge.
Because EF has its own change-tracking, Phlat's change tracking might be considered superfluous (and in future versions this could be optional for marginal performance gains).  


**SETUP**

The below setup is a fairly standard Blog/Post examiple.  Note that tracking is turned off in general, but later when we're querying for 
our blog from the context we use `.AsTracking()`.
```csharp
public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.UseInMemoryDatabase("sampledatabase");
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    
    public abstract class Base
    {
        public int Id { get; set; }

        // override object.Equals so that object equality is determined by id
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Id.Equals(((Base)obj).Id);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Id;
        }
    }

    public class Blog : Base
    {
        public string Url { get; set; }
        public int Rating { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class Post : Base
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
```


**INIT**


This simply loads up our context with our sample Blog.

```csharp
        
/// <summary>
/// Creates dummy data and inserts it inot our BloggingContext
/// </summary>
public static void InitEF()
{
    var blog = new Blog
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

    using (var dbContext = new BloggingContext())
    {
        if (!dbContext.Blogs.Any())
        {
            dbContext.Blogs.AddRange(new[] { blog });
            dbContext.SaveChanges();
        }
    }
}

```

