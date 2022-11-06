using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Parser.Grammar;

public class GrammarRule
{
    public GrammarRule(string ruleName, IEnumerable<IRule> rules)
    {
        RuleName = ruleName.NotEmpty();
        Rules = rules.NotNull().ToArray().Assert(x => x.Length > 0, "Empty list");
    }

    public string RuleName { get; }

    public IReadOnlyList<IRule> Rules { get; }
}
