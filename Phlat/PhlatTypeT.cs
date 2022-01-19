using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Phlatware
{
    public interface IPhlatType
    {
        Action<object, object> Update { get; }
        ISnapshot CreateSnapshot(object model);
    }

    public class PhlatType<T> : IPhlatType
    {
        internal IList<Path<T>> Definitions { get; set; } = new List<Path<T>>();

        public Action<T, T> Update { get; set; }

        internal Snapshot<T> CreateSnapshot(T model) 
        {
            var options = new SnapshotOptions
            {
                BindingFlags = BindingFlags
            };
            return new Snapshot<T>(model, options);
        }
        public BindingFlags BindingFlags { get; set; } = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        Action<object, object> IPhlatType.Update => (s, t) => Update((T)s, (T)t);
        ISnapshot IPhlatType.CreateSnapshot(object model) => CreateSnapshot((T)model);

        public PhlatType()
        {
            //add a default root definition
            var rootDefinition = makeDefinition((t) => new List<T> { t });
            Definitions.Add(rootDefinition);
        }

        /// <summary>
        /// Singular
        /// </summary>
        public PhlatType<T> HasOne<TItem>(
                                            Func<T, TItem> get,
                                            Func<TItem, TItem, bool> delete = null)
        {
            HasMany<TItem>(t => get(t) == null ? null : new List<TItem> { get(t) },
                delete);

            return this;
        }

        /// <summary>
        /// Enumerable
        /// </summary>
        public PhlatType<T> HasMany<TItem>(
                                        Func<T, IList<TItem>> get,
                                        Func<TItem, TItem, bool> delete = null)
        {
            var definition = makeDefinition(get, delete);

            Definitions.Add(definition);

            return this;
        }

        private Path<T> makeDefinition<TItem>(
                                        Func<T, IList<TItem>> get,
                                        Func<TItem, TItem, bool> delete = null)
        {
            return new Path<T>
            {
                Get = (t) => get(t)?.Cast<object>(),
                Insert = (t, ti) => get(t).Add((TItem)ti),
                Delete = (s,t)=> delete?.Invoke((TItem)s,(TItem)t) ?? false,
                Type = typeof(TItem)
            };
        }
    }
}