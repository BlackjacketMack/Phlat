using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        Action<object, object> IPhlatType.Update => (s, t) => Update((T)s, (T)t);


        ISnapshot IPhlatType.CreateSnapshot(object model) => CreateSnapshot((T)model);

        public PhlatType()
        {
        }

        /// <summary>
        /// Singular
        /// </summary>
        public PhlatType<T> HasOne<TItem>(
                                            Func<T, TItem> get,
                                            Func<TItem, TItem, bool> shouldDelete = null)
        {
            HasMany<TItem>(t => get(t) == null ? null : new List<TItem> { get(t) },
                shouldDelete);

            return this;
        }

        /// <summary>
        /// Enumerable
        /// </summary>
        public PhlatType<T> HasMany<TItem>(
                                        Func<T, IList<TItem>> get,
                                        Func<TItem, TItem, bool> shouldDelete = null)
        {
            var definition = makeDefinition(get, shouldDelete);

            Paths.Add(definition);

            return this;
        }

        private Path<T> makeDefinition<TItem>(
                                        Func<T, IList<TItem>> get,
                                        Func<TItem, TItem, bool> shouldDelete = null)
        {
            return new Path<T>
            {
                Get = (t) => get(t)?.Cast<object>(),
                Insert = (t, ti) => get(t).Add((TItem)ti),
                ShouldDelete = (s,t)=> shouldDelete?.Invoke((TItem)s,(TItem)t) ?? false,
                Type = typeof(TItem)
            };
        }
    }
}