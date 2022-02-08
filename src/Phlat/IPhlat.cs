namespace Phlatware
{
    public interface IPhlat
    {
        /// <summary>
        /// Flattens a model with all states indicating unchanged.
        /// If the given property (either singular or enumerable) is null, it will not be included.
        /// </summary>
        ResultList<T> Flatten<T>(T model);

        /// <summary>
        /// Flattens the source model, and then flattens the target model and then compares the results.
        /// The target model will be mutated to match the source model.
        /// </summary>
        ResultList<T> Merge<T>(T sourceModel, T targetModel);
    }
}