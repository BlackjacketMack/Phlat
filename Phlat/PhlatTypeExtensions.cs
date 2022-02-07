using System;
using System.Collections.Generic;

namespace Phlatware
{
    public static class PhlatTypeExtensions
    {
        
        /// <summary>
        /// Simple extension that handles the common scenario of removing items from a target that are not
        /// in the source object.
        /// If Source has A,B,D and Target has A,B,C,D...C should be removed.
        /// </summary>
        public static PhlatType<T> HasMany<T,TItem>(this PhlatType<T> phlatType,
                                            Func<T, IList<TItem>> get,
                                            bool deleteIfMissing)
        {
            return phlatType.HasMany(get, deleteIf: (s, t) => deleteIfMissing ? s == null : false);
        }
    }
}