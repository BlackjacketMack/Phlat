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
        private static IDictionary<string, Func<T, object>> _propertyResolvers;

        private T _original;
        private IDictionary<string, object> _startValues;

        static Snapshot()
        {
            _propertyResolvers = _propertyResolvers ?? buildPropertyResolvers();
        }

        public Snapshot(T original)
        {
            _original = original;
        }


        public IDictionary<string, object> Values()
        {
            return _propertyResolvers.ToDictionary(dict => dict.Key,
                                                  dict => dict.Value(_original),
                                                  StringComparer.InvariantCultureIgnoreCase);
        }

        public IDictionary<string, object> Start()
        {
            _startValues = Values();

            return _startValues;
        }

        private static bool areEqual(object first, object second)
        {
            if (first == null && second == null) return true;
            if (first == null) return false;
            return first.Equals(second);
        }

        /// <summary>
        /// Returns a list of changes
        /// </summary>
        public IDictionary<string,object> Changes()
        {
            var comparedValues = Compare();

            return comparedValues
                    .Where(w => !areEqual(w.Value.OldValue, w.Value.NewValue))
                    .ToDictionary(d => d.Key, d => d.Value.NewValue);
        }

        /// <summary>
        /// Returns all values side by side
        /// </summary>
        public IDictionary<string, (object OldValue, object NewValue)> Compare()
        {
            var currentValues = Values();

            return _startValues.ToDictionary(sv => sv.Key, sv => (sv.Value, currentValues[sv.Key]));
        }

        private static List<PropertyInfo> getRelevantProperties()
        {
            return typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p =>
                    p.GetSetMethod(true) != null &&
                    p.GetGetMethod(true) != null &&
                    (
                    //no focus on the property type
                    p.PropertyType.IsValueType ||

                    //strings are fine
                    p.PropertyType == typeof(string) ||

                    //nullabletypes are fine
                    (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    )
                    ).ToList();
        }

        /// <summary>
        /// Using compiled expressions
        /// http://www.palmmedia.de/Blog/2012/2/4/reflection-vs-compiled-expressions-vs-delegates-performance-comparision
        /// Note that we have to convert the expression because value types don't register right as 'object' args.  
        /// Skeet beautifully tells us how to convert the expression.
        /// http://stackoverflow.com/questions/2200209/expression-of-type-system-int32-cannot-be-used-for-return-type-system-object
        /// </summary>
        public static IDictionary<string, Func<T, object>> buildPropertyResolvers()
        {
            //get the properties in play
            var props = getRelevantProperties();

            //key=propname, value=func for get
            var propResolvers = new Dictionary<string, Func<T, object>>();

            //for each prop, create a compiled expression to get the value of the prop.
            foreach (var prop in props)
            {
                var arg = Expression.Parameter(typeof(T), "x");
                var expr = Expression.Property(arg, prop.Name);
                var conversion = Expression.Convert(expr, typeof(object));
                var propertyResolver = Expression.Lambda<Func<T, object>>(conversion, arg).Compile();

                propResolvers.Add(prop.Name, propertyResolver);
            }

            return propResolvers;
        }
    }
}
