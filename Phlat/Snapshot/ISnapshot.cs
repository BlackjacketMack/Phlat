using System.Collections.Generic;

namespace Phlatware
{
    public interface ISnapshot
    {
        IDictionary<string, object> Start();
       IDictionary<string, object> Values();
       IDictionary<string, object> Changes();
       IDictionary<string, (object OldValue, object NewValue)> Compare();
    }
}
