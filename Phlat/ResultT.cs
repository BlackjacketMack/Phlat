using System;
using System.Collections.Generic;

namespace Phlatware
{
    public class Result<T>
    {
        /// <summary>
        /// The root model this result was derived from.  For the root model, the root and model will be the same.
        /// </summary>
        public T Root { get; set; }
        public object Model { get; set; }
        public bool IsRoot => (object)this.Root == this.Model;
        public Type Type => this.Model.GetType();
        public IDictionary<string, object> Values { get; set; }
        public IDictionary<string, object> Changes { get; set; }
        public ResultStates State { get; set; }

        internal Path<T> Definition { get; set; }
    }
}