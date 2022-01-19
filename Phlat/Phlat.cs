namespace Phlatware
{
    public class Phlat : IPhlat
    {
        private readonly PhlatConfiguration _configuration;

        public Phlat(PhlatConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ResultList<T> Flatten<T>(T model, bool includeValues = false)
        {
            return new Phlat<T>(_configuration).Flatten(model, includeValues);
        }

        public ResultList<T> Create<T>(T sourceModel)
        {
            return new Phlat<T>(_configuration).Create(sourceModel);
        }

        public ResultList<T> Modify<T>(T sourceModel, T targetModel)
        {
            return new Phlat<T>(_configuration).Modify(sourceModel, targetModel);
        }
    }
}