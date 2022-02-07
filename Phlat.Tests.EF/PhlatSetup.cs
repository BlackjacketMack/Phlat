namespace Phlatware.Tests.EF
{
    public class PhlatSetup
    {
        public static Phlat ConfigurePhlat()
        {
            var config = new PhlatConfiguration();
            config.Configure<Blog>((s, t) =>                         //configure blog the simple properties you want set when the source blog is applied to the target blog
            {
                t.Url = s.Url;
                t.Rating = s.Rating;
            })
            .HasMany(
                b => b.Posts,                                       //define the path
                deleteIfMissing:true                            
            );                                         

            config.Configure<Post>((s, t) =>
            {
                t.Content = s.Content;
                t.Title = s.Title;
            });

            return new Phlat(config);
        }
    }
    
}
