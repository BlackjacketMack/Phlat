using System;
using System.Collections.Generic;

namespace Phlatware
{
    public class Change
    {
        public string Name { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}