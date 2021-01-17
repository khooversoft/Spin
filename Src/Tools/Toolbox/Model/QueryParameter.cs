using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Model
{
    public record QueryParameter
    {
        public int Index { get; init; } = 0;

        public int Count { get; init; } = 1000;

        public string? Namespace { get; init; }

        public string? Filter { get; init; }

        public static QueryParameter Default { get; } = new QueryParameter();
    }
}
