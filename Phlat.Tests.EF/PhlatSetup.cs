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
                deleteIf:(s,t)=>s == default                             //indicate that if a source item is missing (null) it should be flagged as deleted
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
