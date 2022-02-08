using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Phlatware
{
    /// <summary>
    /// Used solely to wrap a generic equality comparer and implement the non-generic
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    internal class CustomEqualityComparer<TItem> : IEqualityComparer
    {
        private readonly IEqualityComparer<TItem> _custom;

        public CustomEqualityComparer(IEqualityComparer<TItem> custom)
        {
            _custom = custom;
        }

        public new bool Equals(object x, object y)
        {
            var comparer = _custom ?? EqualityComparer<TItem>.Default;
            if (x?.GetType() == y?.GetType())
                return comparer.Equals((TItem)x, (TItem)y);
            else
                return false;
        }
        public int GetHashCode(object obj) => _custom.GetHashCode((TItem)obj);
    }
}