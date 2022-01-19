using System;
using System.Collections.Generic;
using System.Linq;

namespace Phlatware
{
    internal class Phlat<T> : Phlat
    {
        private readonly PhlatType<T> _rootType;
        private readonly PhlatConfiguration _configuration;

        private IDictionary<Type, IPhlatType> _registry => _configuration.Registry;

        private IPhlatType getType(Type type)
        {
            if (!_registry.TryGetValue(type, out IPhlatType val))
                throw new ApplicationException("The type you specified is not registerd.");

            return val;
        }

        public Phlat(PhlatConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;

            _rootType = getType(typeof(T)) as PhlatType<T>;
        }

        /// <summary>
        /// <see cref="Phlat.Flatten" />
        /// </summary>
        public ResultList<T> Flatten(T model, bool includeValues = false)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var results = new ResultList<T>();

            foreach (var definition in _rootType.Definitions)
            {
                _registry.TryGetValue(definition.Type, out IPhlatType itemConfig);

                var dataModels = definition.Get(model);

                if (dataModels == null) continue;

                foreach (var dataModel in dataModels)
                {
                    var snapshot = itemConfig.CreateSnapshot(dataModel);

                    var values = includeValues ? snapshot.Values() : null;

                    var result = new Result<T>
                    {
                        Root = model,
                        Model = dataModel,
                        Definition = definition,
                        Values = values,
                        Changes = null
                    };

                    results.Add(result);
                }
            }

            return results;
        }

        public ResultList<T> Create(T leftModel)
        {
            if (leftModel == null) throw new ArgumentNullException(nameof(leftModel));

            var results = Flatten(leftModel);

            foreach(var result in results)
            {
                result.State = ResultStates.Created;
            }

            return results;
        }
        
        public ResultList<T> Modify(T leftModel, T rightModel)
        {
            if (leftModel == null) throw new ArgumentNullException(nameof(leftModel));
            if (rightModel == null) throw new ArgumentNullException(nameof(rightModel));
            if (Object.ReferenceEquals(leftModel, rightModel)) throw new ArgumentException("Left and right models are equal.");
            
            var leftResults = Flatten(leftModel);
            var rightResults = Flatten(rightModel);
            var returnResults = new ResultList<T>();

            foreach (var leftResult in leftResults)
            {
                var definition = leftResult.Definition;

                _registry.TryGetValue(definition.Type, out IPhlatType itemConfig);

                //this will be our return result
                var rightResult = rightResults.Where(rr => Object.Equals(leftResult.Model, rr.Model)).SingleOrDefault();

                IDictionary<string, object> changes = new Dictionary<string, object>();

                //insert
                if (rightResult == null)
                {
                    definition.Insert(rightModel, leftResult.Model);

                    var snapshot = itemConfig.CreateSnapshot(leftResult.Model);

                    rightResult = new Result<T>
                    {
                        Model = leftResult.Model,
                        State = ResultStates.Created,
                        Values = snapshot.Values(),
                        Changes = changes,
                        Definition = definition
                    };
                }
                //update
                else if(leftResult.State != ResultStates.Deleted)
                {
                    var snapshot = itemConfig.CreateSnapshot(rightResult.Model);
                    var startValues = snapshot.Start();

                    itemConfig.Update(leftResult.Model, rightResult.Model);

                    rightResult.Values = startValues;
                    rightResult.Changes = snapshot.Changes();
                }

                rightResult.State = leftResult.State;

                handleState(leftResult, rightResult);

                returnResults.Add(rightResult);
            }

            //delete items works from the target to the source
            foreach(var rightResult in rightResults)
            {
                var definition = rightResult.Definition;

                var leftResult = leftResults.Where(lr => Object.Equals(rightResult.Model, lr.Model)).SingleOrDefault();

                if (definition.Delete(leftResult?.Model, rightResult.Model))
                {
                    rightResult.State = ResultStates.Deleted;
                    returnResults.Add(rightResult);
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

        private void handleState(Result<T> leftResult, Result<T> rightResult)
        {
            //changes are determined by the rightresult(the return result)
            var changesExist = rightResult.Changes?.Any() ?? false;

            //copy modelstate
            if (rightResult.State == ResultStates.Deleted)
            {
                //don't do diddly
            }
            //insert
            else if (rightResult.Model == null)
            {
                rightResult.State = ResultStates.Created;
            }
            //update
            else if (changesExist)
            {
                rightResult.State = ResultStates.Updated;
            }
            //no change
            else
            {
                rightResult.State = ResultStates.Unchanged;
            }

            //the left result should mirror the modelstates we've set on the right result
            leftResult.State = rightResult.State;
        }
    }
}