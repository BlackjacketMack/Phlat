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
            sb.AppendLine("| IsRoot | Model | State | Values | Changes |");
            sb.AppendLine("| ------ | ----- | ----- | ------ | ------- |");

            foreach (var result in this)
            {
                sb.AppendLine($"| {result.IsRoot} | {result.Model} | {result.State} | {dictToString(result.Values)} | {dictToString(result.Changes)} |");
            }

            return sb.ToString();
        }

        private string dictToString(IDictionary<string, object> dict) => dict == null ? null : String.Join(",", dict);
    }
}