using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.BlockDocument
{
    public abstract class BlockBase
    {
        protected BlockBase(IEnumerable<KeyValuePair<string, string>>? properties)
        {
            Properties = properties?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();
        }

        public IReadOnlyDictionary<string, string> Properties { get; }

        public override string ToString()
        {
            return Properties
                .Aggregate(string.Empty, (a, x) => a += $"{x.Key}={x.Value}");
        }

        public override bool Equals(object? obj)
        {
            return obj is BlockBase subject &&
                Enumerable.SequenceEqual(Properties.Keys.OrderBy(x => x), subject.Properties.Keys.OrderBy(x => x)) &&
                Enumerable.SequenceEqual(Properties.Values.OrderBy(x => x), subject.Properties.Values.OrderBy(x => x));
        }

        public override int GetHashCode() => HashCode.Combine(Properties);

        public static bool operator ==(BlockBase? left, BlockBase? right) => EqualityComparer<BlockBase>.Default.Equals(left, right);

        public static bool operator !=(BlockBase? left, BlockBase? right) => !(left == right);
    }
}
