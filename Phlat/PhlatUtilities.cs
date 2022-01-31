using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Phlatware
{
    internal static class PhlatUtilities
    {
        /// <summary>
        /// Convert a lambda expression for a getter into a setter
        /// From https://stackoverflow.com/questions/8107134/how-set-value-a-property-selector-expressionfunct-tresult
        /// </summary>
        public static Action<T, TProperty> GetSetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var setMethod = property.GetSetMethod();

            var parameterT = Expression.Parameter(typeof(T), "x");
            var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

            var newExpression =
                Expression.Lambda<Action<T, TProperty>>(
                    Expression.Call(parameterT, setMethod, parameterTProperty),
                    parameterT,
                    parameterTProperty
                );

            return newExpression.Compile();
        }
    }
}