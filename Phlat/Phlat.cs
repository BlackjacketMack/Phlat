namespace Phlatware
{
    public class Phlat : IPhlat
    {
        private readonly PhlatConfiguration _configuration;

        public Phlat(PhlatConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ResultList<T> Flatten<T>(T model)
        {
            return new Phlat<T>(_configuration).Flatten(model);
        }

        public ResultList<T> Merge<T>(T sourceModel, T targetModel)
        {
            return new Phlat<T>(_configuration).Merge(sourceModel, targetModel);
        }
    }
}