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
            var pathName = PhlatUtilities.GetMemberName(get);
            var getCompiled = get.Compile();
            var setCompiled = PhlatUtilities.GetSetter(get);
            addPath<TItem>(
                    pathName,
                    t => {
                        var v = getCompiled.Invoke(t);
                        return v != null ? new List<TItem> { v } : null;
                    },
                    setCompiled,
                    shouldDelete,
                    delete: (t,ti)=>setCompiled(t,default));

            return this;
        }

        /// <summary>
        /// Returns and sets a list of properties.  
        /// </summary>
        public PhlatType<T> HasMany<TItem>(
                                        Expression<Func<T, IList<TItem>>> get,
                                        Func<TItem, TItem, bool> deleteIf = null)
        {
            var pathName = PhlatUtilities.GetMemberName(get);
            var getCompiled = get.Compile();
            addPath(pathName,
                    getCompiled,
                    (t,ti)=> getCompiled(t).Add(ti),
                    deleteIf,
                    (t,ti)=> getCompiled(t).Remove(ti));

            return this;
        }

        private void addPath<TItem>(
                                        string name,
                                        Func<T, IList<TItem>> get,
                                        Action<T,TItem> insertAction,
                                        Func<TItem, TItem, bool> shouldDelete = null,
                                        Action<T,TItem> delete = null)
        {
            var path = new Path<T>
            {
                Name = name,
                Get = (t) => get(t)?.Cast<object>(),
                Insert = (t, ti) => insertAction(t,(TItem)ti),
                DeleteIf = (s,t)=> shouldDelete?.Invoke((TItem)s,(TItem)t) ?? false,
                Delete = (t,ti) => delete(t,(TItem)ti),
                ItemType = typeof(TItem)
            };

            Paths.Add(path);
        }
    }
}