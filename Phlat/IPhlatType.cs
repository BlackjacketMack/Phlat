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

        /// <summary>
        /// All paths, including the root path (the first)
        /// </summary>
        IEnumerable<IPath> Paths { get; }
    }
}