using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Pattern
{
    public class PatternContext
    {
        public string Source { get; init; } = null!;

        public ConcurrentDictionary<string, object> Properties { get; } = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }
}
