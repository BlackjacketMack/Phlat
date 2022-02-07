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
                                            bool deleteIfSourceMissing)
        {
            return phlatType.HasMany(get, deleteIf: (s, t) => deleteIfSourceMissing ? s == null : false);
        }

        /// <summary>
        /// Simple extension that handles the common scenario of removing items 
        /// based on a flag property (e.g. IsDeleted).  This extension is slightly cleaner
        /// than using the root Action which allows evaluation of both the source and target items.
        /// This extension looks at only the source item.
        /// </summary>
        public static PhlatType<T> HasMany<T, TItem>(this PhlatType<T> phlatType,
                                            Func<T, IList<TItem>> get,
                                            Func<TItem,bool> deleteIfSourceHas)
        {
            if (deleteIfSourceHas == null) throw new ArgumentNullException(nameof(deleteIfSourceHas));

            return phlatType.HasMany(get, deleteIf: (s, t) => deleteIfSourceHas(s));
        }
    }
}