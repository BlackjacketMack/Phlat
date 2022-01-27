using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Phlatware.Tests.EF
{
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

        // override object.Equals
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

}