using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Parser.Grammar;

public record RuleNode : IRule
{
    public required string Name { get; init; }
}
