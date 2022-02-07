using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phlatware
{
    public class ResultList<T> : List<Result<T>>
    {
        public Result<T> RootResult => this.Single(s => s.IsRoot);
        public IEnumerable<Result<T>> MemberResults => this.Where(w => !w.IsRoot);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("| Path | Model | State | Values | Updates |");
            sb.AppendLine("| ------ | ----- | ----- | ------ | ------- |");

            foreach (var result in this)
            {
                var updates = result.Updates?.ToDictionary(e => e.Key, e => (object)new { e.Value.OldValue, e.Value.NewValue });
                sb.AppendLine($"| {result.PathName} | {result.Model} | {result.State} | `{dictToString(result.Values)}` | `{dictToString(updates)}` |");
            }

            return sb.ToString();
        }

        private string dictToString(IDictionary<string, object> dict) => dict == null ? null : String.Join(",", dict);
    }
}