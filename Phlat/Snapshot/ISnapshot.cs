using System.Collections.Generic;

namespace Phlatware
{
    public interface ISnapshot
    {
        /// <summary>
        /// Simple read of property values
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> Values();
    }
}
