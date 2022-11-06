using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tokenizer.Token;

namespace Toolbox.Parser.Grammar;

public class IntRule : IRule
{
    public int Value { get; }

    public IntRule(int value) => Value = value;

    public static IntRule? Match(IToken token) => token switch
    {
        TokenValue v => int.TryParse(v, out _) switch
        {
            false => null,
            true => new IntRule(int.Parse(v)),
        },

        _ => null,
    };
}
