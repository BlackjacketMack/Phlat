using System;
using System.Collections.Generic;
using System.Linq;

namespace Phlatware
{
    public class Result<T>
    {
        /// <summary>
        /// The root model this result was derived from.  For the root model, the root and model will be the same.
        /// </summary>
        public T Root { get; set; }
        public object Parent { get; set; }
        public object Model { get; set; }
        public bool IsRoot => (object)this.Root == this.Model;
        public Type Type => this.Model.GetType();

        /// <summary>
        /// The current values of the model
        /// </summary>
        public IDictionary<string, object> Values { get; set; }

        /// <summary>
        /// Updated values from a merge operation
        /// </summary>
        public IDictionary<string,(object OldValue, object NewValue)> Updates { get; set; }

        /// <summary>
        /// Only the new values from the Updates dictionary
        /// </summary>
        public IDictionary<string, object> Changes => Updates?.ToDictionary(e => e.Key, e => e.Value.NewValue);

        public ResultStates State { get; set; }
        internal IPath Path { get; set; }

        public override string ToString() => $"{Model} {State} (Parent: {Parent})";
    }
}