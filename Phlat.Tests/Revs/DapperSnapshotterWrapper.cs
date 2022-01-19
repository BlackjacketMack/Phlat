using Dapper;
using Phlatware;
using System.Collections.Generic;

namespace Phlat.Tests.Benchmarks
{
    public class DapperSnapshotterWrapper<T> : ISnapshot
    {
        private readonly T _original;

        public IDictionary<string, object> Changes()
        {
            var diff = _snapshot.Diff();

            var dict = new Dictionary<string, object>();

            foreach(var name in diff.ParameterNames)
            {
                dict.Add(name, diff.Get<object>(name));
            }

            return dict;
        }

        public IDictionary<string, (object OldValue, object NewValue)> Compare()
        {
            throw new System.NotImplementedException();
        }

        public IDictionary<string, object> Start()
        {
            _snapshot = Snapshotter.Start(_original);

            return null;
        }

        public IDictionary<string, object> Values()
        {
            throw new System.NotImplementedException();
        }

        private Snapshotter.Snapshot<T> _snapshot;

        public DapperSnapshotterWrapper(T original)
        {
            _original = original;
        }
    }
}
