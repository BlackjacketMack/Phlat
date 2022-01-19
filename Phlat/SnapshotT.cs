using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Phlatware
{

    /// <summary>
    /// Much of this is graciously inspired by Dapper.Rainbow's snapshotter.
    /// </summary>
    internal class Snapshot<T> : ISnapshot
    {
        private static Lazy<IDictionary<string, Func<T, object>>> _propertyResolvers;

        private T _original;
        private readonly SnapshotOptions _options;

        private IDictionary<string, object> _startValues; 

        public Snapshot(T original, SnapshotOptions options = null)
        {
            _original = original;
            _options = options ?? new SnapshotOptions();
            _propertyResolvers = _propertyResolvers ?? new Lazy<IDictionary<string, Func<T, object>>>(() => buildPropertyResolver());
        }

        public IDictionary<string, object> Values()
        {
            return _propertyResolvers.Value
                                    .ToDictionary(dict => dict.Key,
                                                  dict => dict.Value(_original));
        }

        public IDictionary<string, object> Start()
        {
            _startValues = Values();

            return _startValues;
        }

        /// <summary>
        /// Returns a list of changes
        /// </summary>
        public IDictionary<string,object> Changes()
        {
            var comparedValues = Compare();

            var changes = comparedValues
                    .Where(w => !Object.Equals(w.Value.OldValue, w.Value.NewValue))
                    .ToDictionary(d => d.Key, d => d.Value.NewValue);

            return changes;
        }

        /// <summary>
        /// Returns all values side by side
        /// </summary>
        public IDictionary<string, (object OldValue, object NewValue)> Compare()
        {
            var currentValues = Values();

            return _startValues.ToDictionary(sv => sv.Key, sv => (sv.Value, currentValues[sv.Key]));
        }

        /// <summary>
        /// Using compiled expressions
        /// http://www.palmmedia.de/Blog/2012/2/4/reflection-vs-compiled-expressions-vs-delegates-performance-comparision
        /// Note that we have to convert the expression because value types don't register right as 'object' args.  
        /// Skeet beautifully tells us how to convert the expression.
        /// http://stackoverflow.com/questions/2200209/expression-of-type-system-int32-cannot-be-used-for-return-type-system-object
        /// </summary>
        public IDictionary<string, Func<T, object>> buildPropertyResolver()
        {
            //get the properties in play
            var props = typeof(T).GetProperties(_options.BindingFlags)
                                        .Where(w => w.CanRead)
                                        .Where(w =>
                                            //'include' is a whitelist...no type evaluation needed if it's on the list
                                            //include.Contains(w.Name) ||

                                            //no focus on the property type
                                            w.PropertyType.IsValueType ||

                                            //strings are fine
                                            w.PropertyType == typeof(string) ||

                                            //nullabletypes are fine
                                            (w.PropertyType.IsGenericType && w.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)));

            //key=propname, value=func for get
            var propertyResolverDictionary = new Dictionary<string, Func<T, object>>();

            //for each prop, create a compiled expression to get the value of the prop.
            foreach (var prop in props)
            {
                var arg = Expression.Parameter(typeof(T), "x");
                var expr = Expression.Property(arg, prop.Name);
                var conversion = Expression.Convert(expr, typeof(object));
                var propertyResolver = Expression.Lambda<Func<T, object>>(conversion, arg).Compile();

                propertyResolverDictionary.Add(prop.Name, propertyResolver);
            }

            return propertyResolverDictionary;
        }
    }
}
