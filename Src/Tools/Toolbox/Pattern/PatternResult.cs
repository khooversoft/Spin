using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Pattern
{
    public record PatternResult : IPatternResult
    {
        public string Name { get; init; } = null!;

        public string Pattern { get; init; } = null!;

        public string Source { get; init; } = null!;

        public string? Transform { get; init; }

        public ConcurrentDictionary<string, string> Values { get; init; } = null!;
    }
}
