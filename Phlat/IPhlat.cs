namespace Phlatware
{
    public interface IPhlat
    {
        /// <summary>
        /// Flattens a model with all states indicating unchanged.
        /// If the given property (either singular or enumerable) is null, it will not be included.
        /// Values can be rendered into a dictionary if 'includeValues' is set to true.
        /// </summary>
        ResultList<T> Flatten<T>(T model, bool includeValues = false);

        /// <summary>
        /// This is a self-merge which basically handles modelstate.
        /// This is the same as calling merge with left and right being the same instance.
        /// </summary>
        ResultList<T> Create<T>(T sourceModel);

        /// <summary>
        /// Renders the results for each model and combines them.
        /// This will merge the leftModel into the rightModel and return the rightModel.
        /// If the rightmodel is null, it will assume the reference of the leftmodel.
        /// </summary>
        ResultList<T> Modify<T>(T sourceModel, T targetModel);
    }
}