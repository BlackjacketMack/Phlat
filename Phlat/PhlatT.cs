using System;
using System.Collections.Generic;
using System.Linq;

namespace Phlatware
{
    internal class Phlat<T> : Phlat
    {
        private readonly PhlatConfiguration _configuration;

        private T _root;

        public Phlat(PhlatConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// <see cref="Phlat.Flatten" />
        /// </summary>
        public ResultList<T> Flatten(T model)
        {
            return flattenRoot(model, includeValues: true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="includeValues">
        /// Used internally for performance.
        /// </param>
        /// <returns></returns>
        private ResultList<T> flattenRoot(T root, bool includeValues = false)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            _root = root;

            var results = flattenModel(root, includeValues);

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="includeValues"></param>
        /// <param name="parent"></param>
        /// <param name="parentPath"></param>
        /// <returns></returns>
        private ResultList<T> flattenModel(
                object model, 
                bool includeValues = false, 
                object parent = null, 
                IPath parentPath = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var results = new ResultList<T>();

            var type = _configuration.GetPhlatType(model.GetType());

            if (type == null)
                throw new ApplicationException($"A phlattype is not configured for model type {model.GetType()}");

            var snapshot = type.CreateSnapshot(model);

            var values = includeValues ? snapshot.Values() : null;

            var result = new Result<T>
            {
                Root = _root,
                Parent = parent,
                Model = model,
                Path = parentPath,                    //how it got here
                Values = values,
                Changes = null
            };

            results.Add(result);

            foreach (var path in type.Paths)
            {
                var dataModels = path.Get(model);

                if (dataModels == null) continue;

                foreach (var dataModel in dataModels)
                {
                    var nestedResults = flattenModel(dataModel, 
                                                includeValues: includeValues, 
                                                parent: model,
                                                parentPath: path);

                    results.AddRange(nestedResults);
                }
            }

            return results;
        }
        
        public ResultList<T> Merge(T sourceModel, T targetModel)
        {
            if (sourceModel == null) throw new ArgumentNullException(nameof(sourceModel));
            if (targetModel == null) throw new ArgumentNullException(nameof(targetModel));
            if (Object.ReferenceEquals(sourceModel, targetModel)) throw new ArgumentException("Left and right models are equal.");
            
            //get source and target results flattened.
            //We don't need to include values because they're calculated individually per operation (which results in a modest performance boost)
            var sourceResults = flattenRoot(sourceModel,includeValues:false);
            var targetResults = flattenRoot(targetModel,includeValues: false);
            var returnResults = new ResultList<T>();

            foreach (var sourceResult in sourceResults)
            {
                var path = sourceResult.Path;

                var sourcePhlatType = _configuration.GetPhlatType(sourceResult.Type);

                //this will be our return result
                var targetResult = targetResults.Where(rr => Object.Equals(sourceResult.Model, rr.Model)).SingleOrDefault();

                IDictionary<string, object> changes = new Dictionary<string, object>();

                //insert
                if (targetResult == null)
                {
                    //find the parent by ref (if newly added) or equality
                    var parent = returnResults.Single(rr => rr.Model == sourceResult.Parent || rr.Model.Equals(sourceResult.Parent)).Model;

                    //we quickly check that the model isn't already added
                    if(!path.Get(parent)?.Any(m=>m == sourceResult.Model) ?? true)
                        path.Insert(parent, sourceResult.Model);

                    var snapshot = sourcePhlatType.CreateSnapshot(sourceResult.Model);

                    targetResult = new Result<T>
                    {
                        Root = _root,
                        Parent = parent,
                        Model = sourceResult.Model,
                        State = ResultStates.Created,
                        Values = snapshot.Values(),
                        Changes = changes,
                        Path = path
                    };
                }
                
                //if this is the root result it should be passed through the update routing.
                //otherwise, if it is a sub-item check if we should delete it.
                else if(sourceResult.IsRoot || !path.ShouldDelete(sourceResult?.Model, targetResult.Model))
                {
                    var snapshot = sourcePhlatType.CreateSnapshot(targetResult.Model);
                    var startValues = snapshot.Start();

                    if (sourcePhlatType.Update == null)
                        throw new ApplicationException("No update method defined.");

                    sourcePhlatType.Update(sourceResult.Model, targetResult.Model);

                    targetResult.Values = startValues;
                    targetResult.Changes = snapshot.Changes();

                    if (targetResult.Changes.Any())
                        targetResult.State = ResultStates.Updated;
                }

                returnResults.Add(targetResult);
            }

            //delete items works from the target to the source
            foreach(var targetResult in targetResults)
            {
                if (targetResult.IsRoot) continue;

                var path = targetResult.Path;

                var sourceResult = sourceResults.Where(lr => Object.Equals(targetResult.Model, lr.Model)).SingleOrDefault();

                if (path.ShouldDelete(sourceResult?.Model, targetResult.Model))
                {
                    var targetPhlatType = _configuration.GetPhlatType(targetResult.Type);
                    var snapshot = targetPhlatType.CreateSnapshot(targetResult.Model);
                    var targetValues = snapshot.Start();
                    targetResult.Values = targetValues;

                    targetResult.State = ResultStates.Deleted;
                    returnResults.Add(targetResult);
                }
            }

            //finally, if any result has a modelstate of not unchanged, mark the root as changed.
            if (returnResults.Any(r => r.State != ResultStates.Unchanged))
            {
                //set the root (first) to tobeedited
                returnResults[0].State = ResultStates.Updated;
            }

            return returnResults;
        }


    }
}