using System;
using System.Collections.Generic;

namespace Phlatware
{
    public interface IPhlatType
    {
        Action<object, object> Update { get; }
        ISnapshot CreateSnapshot(object model);

        /// <summary>
        /// All paths, including the root path (the first)
        /// </summary>
        IEnumerable<IPath> Paths { get; }
    }
}