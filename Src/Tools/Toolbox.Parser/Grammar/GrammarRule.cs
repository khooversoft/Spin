using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Parser.Grammar;

public class GrammarRule
{
    private static IReadOnlyList<Func<Cursor<IToken>, IReadOnlyList<IRule>?>> _ruleFunctions = new Func<Cursor<IToken>, IReadOnlyList<IRule>?>[]
    {
        c => IntRule.Match(c),
    };

    public GrammarRule(string name, IEnumerable<IRule> rules)
    {
        Name = name.NotEmpty();
        Rules = rules.NotNull().ToArray().Assert(x => x.Length > 0, "Empty list");
    }

    public string Name { get; }

    public IReadOnlyList<IRule> Rules { get; }

    //public static GrammarRule? Match(Cursor<IToken> tokenCursor)
    //{
    //    List<IRule> ruleList = new();


    //}
}
