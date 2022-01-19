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
            var tabs = new string('\t', 5);
            sb.Append("Name         " + tabs);
            sb.Append("State        " + tabs);
            sb.Append("Values       " + tabs);

            foreach (var result in this)
            {
                sb.AppendLine();
                sb.Append(result.Model + tabs);
                sb.Append(result.State + tabs);
                sb.AppendLine(String.Empty);
                if (result.Values?.Any() ?? false)
                    sb.AppendLine(tabs + "Values:" + String.Join(",", result.Values) + tabs);

                if (result.Changes?.Any() ?? false)
                    sb.AppendLine(tabs + "Changes:" + String.Join(",", result.Changes) + tabs);
            }

            return sb.ToString();
        }
    }
}