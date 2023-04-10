using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Pattern
{
    public record PatternSearch
    {
        public string Name { get; init; } = null!;

        public string Pattern { get; init; } = null!;

        public bool Resolve { get; init; }
    }
}
