using System;
using System.Collections;
using System.Collections.Generic;

namespace Phlatware
{
    /// <summary>
    /// Represents the configuration for either the root or an underlying property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Path<T> : IPath
    {
        Func<object, IEnumerable<object>> IPath.Get => o => Get((T)o);
        Action<object, object> IPath.Insert => (t, obj) => Insert((T)t, obj);
        Action<object, object> IPath.Delete => (t,ti) => Delete((T)t,ti);

        /// <summary>
        /// Get one or more models from T
        /// </summary>
        public Func<T, IEnumerable<object>> Get { get; set; }

        /// <summary>
        /// Insert (or set) a member back to T
        /// </summary>
        public Action<T, object> Insert { get; set; }

        /// <summary>
        /// Evaluates the source against the target and determines if the source should be marked for deletion.
        /// IF you simply want to delete items that have been removed from the source, you can set this to be:
        /// (s,t)=>s == null (because there's a target item and no matching source item)
        /// 
        /// If you want to delete based on a state property, you can do that too.
        /// (s,t)=>s.IsDeleted
        /// </summary>
        public Func<object, object, bool> ShouldDelete { get; set; }

        public Action<T,object> Delete { get; set; }

        public Type Type { get; set; }

    }
}