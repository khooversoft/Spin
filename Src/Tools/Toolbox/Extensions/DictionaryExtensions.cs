using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool IsEqual(this IReadOnlyDictionary<string, string> left, IReadOnlyDictionary<string, string> right)
        {
            if (left == null || right == null) return false;
            if (left.Count != right.Count) return false;

            return left.OrderBy(x => x.Key)
                .Zip(right.OrderBy(x => x.Key), (o, i) => (o, i))
                .All(x => x.o.Key == x.i.Key && x.o.Value == x.i.Value);
        }
    }
}
