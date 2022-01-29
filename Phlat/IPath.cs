using System;
using System.Collections;
using System.Collections.Generic;

namespace Phlatware
{
    /// <summary>
    /// Represents the configuration for either the root or an underlying property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPath
    {
        /// <summary>
        /// Get one or more models from T
        /// </summary>
        Func<object, IEnumerable<object>> Get { get; }

        /// <summary>
        /// Insert (or set) a member back to T
        /// </summary>
        Action<object, object> Insert { get;  }

        /// <summary>
        /// Evaluates the source against the target and determines if the source should be marked for deletion.
        /// IF you simply want to delete items that have been removed from the source, you can set this to be:
        /// (s,t)=>s == null (because there's a target item and no matching source item)
        /// 
        /// If you want to delete based on a state property, you can do that too.
        /// (s,t)=>s.IsDeleted
        /// </summary>
        Func<object, object, bool> ShouldDelete { get;  }

        Type Type { get; }
    }
}