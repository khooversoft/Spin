using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

namespace Toolbox.Parser.Grammar;

public class LiteralRule : IRule
{
    public LiteralRule(string value) => Value = value;

    public string Value { get; }

    public static LiteralRule? Match(IToken token) => token switch
    {
        BlockToken v => new LiteralRule(v.Value),
        _ => null,
    };
}
