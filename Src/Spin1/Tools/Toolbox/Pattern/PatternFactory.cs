using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Pattern
{
    internal class PatternFactory
    {
        public string Name { get; init; } = null!;

        public Func<PatternContext, object?> Factory { get; init; } = null!;
    }
}
