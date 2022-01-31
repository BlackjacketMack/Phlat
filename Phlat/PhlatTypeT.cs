using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Phlatware
{
    public class PhlatType<T> : IPhlatType
    {
        IEnumerable<IPath> IPhlatType.Paths => Paths.Cast<IPath>();
        internal IList<Path<T>> Paths { get; set; } = new List<Path<T>>();

        public Action<T, T> Update { get; set; }

        internal Snapshot<T> CreateSnapshot(T model) 
        {
            return new Snapshot<T>(model);
        }

        Action<object, object> IPhlatType.Update => (s, t) => Update?.Invoke((T)s, (T)t);


        ISnapshot IPhlatType.CreateSnapshot(object model) => CreateSnapshot((T)model);

        public PhlatType()
        {
        }

        /// <summary>
        /// Returns a singular property.
        /// </summary>
        public PhlatType<T> HasOne<TItem>(
                                            Expression<Func<T, TItem>> get,
                                            Func<TItem, TItem, bool> shouldDelete = null)
        {
            var getCompiled = get.Compile();
            var set = PhlatUtilities.GetSetter(get);
            addPath<TItem>(
                    t => {
                        var v = getCompiled.Invoke(t);
                        return v != null ? new List<TItem> { v } : null;
                    },
                    set,
                    shouldDelete);

            return this;
        }

        /// <summary>
        /// Returns and sets a list of properties.  
        /// </summary>
        public PhlatType<T> HasMany<TItem>(
                                        Func<T, IList<TItem>> get,
                                        Func<TItem, TItem, bool> shouldDelete = null)
        {
            addPath(get,
                    (t,ti)=>get(t).Add(ti),
                    shouldDelete);

            return this;
        }

        private void addPath<TItem>(
                                        Func<T, IList<TItem>> get,
                                        Action<T,TItem> insertAction,
                                        Func<TItem, TItem, bool> shouldDelete = null)
        {
            var path = new Path<T>
            {
                Get = (t) => get(t)?.Cast<object>(),
                Insert = (t, ti) => insertAction(t,(TItem)ti),
                ShouldDelete = (s,t)=> shouldDelete?.Invoke((TItem)s,(TItem)t) ?? false,
                Type = typeof(TItem)
            };

            Paths.Add(path);
        }
    }
}