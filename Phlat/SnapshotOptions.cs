using System.Collections.Generic;
using System.Reflection;

namespace Phlatware
{
    /// <summary>
    /// Utility class to make calling modelsnapshot easier.
    /// </summary>
    public class SnapshotOptions
    {
        public BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    }
}
