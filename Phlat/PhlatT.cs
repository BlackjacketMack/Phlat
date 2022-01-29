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
        public ResultList<T> Flatten(T model, bool includeValues = false)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            _root = model;

            var results = flatten(model, includeValues);

            return results;
        }

        /// <summary>
        /// <see cref="Phlat.Flatten" />
        /// </summary>
        private ResultList<T> flatten(
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
                    var nestedResults = flatten(dataModel, 
                                                includeValues: includeValues, 
                                                parent: model,
                                                parentPath: path);
                    results.AddRange(nestedResults);
                }
            }

            return results;
        }

        public ResultList<T> Create(T leftModel)
        {
            if (leftModel == null) throw new ArgumentNullException(nameof(leftModel));

            var results = Flatten(leftModel, includeValues:true);

            foreach(var result in results)
            {
                result.State = ResultStates.Created;
            }

            return results;
        }
        
        public ResultList<T> Modify(T sourceModel, T targetModel)
        {
            if (sourceModel == null) throw new ArgumentNullException(nameof(sourceModel));
            if (targetModel == null) throw new ArgumentNullException(nameof(targetModel));
            if (Object.ReferenceEquals(sourceModel, targetModel)) throw new ArgumentException("Left and right models are equal.");
            
            var sourceResults = Flatten(sourceModel);
            var targetResults = Flatten(targetModel);
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
                    if(!path.Get(parent).Any(m=>m == sourceResult.Model))
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
                //update
                else if(sourceResult.State != ResultStates.Deleted)
                {
                    var snapshot = sourcePhlatType.CreateSnapshot(targetResult.Model);
                    var startValues = snapshot.Start();

                    sourcePhlatType.Update(sourceResult.Model, targetResult.Model);

                    changes = snapshot.Changes();
                    targetResult.Values = startValues;
                    targetResult.Changes = changes;

                    if (changes.Any())
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