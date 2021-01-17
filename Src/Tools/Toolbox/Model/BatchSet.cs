using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Model
{
    public record BatchSet<T>
    {
        public QueryParameter QueryParameter { get; init; } = null!;

        public int NextIndex { get; init; }

        public IReadOnlyList<T> Records { get; init; } = null!;
    }
}
