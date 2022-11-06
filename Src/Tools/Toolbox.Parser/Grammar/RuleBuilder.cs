using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Monads;
using Toolbox.Parser.Application;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

namespace Toolbox.Parser.Grammar;

public class RuleBuilder
{
    public static GrammarRule Build(IEnumerable<string> lines)
    {
        const string syntaxErrorText = "syntax error";

        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .Add("=", "|")
            .Parse(lines.Join(" "));

        Stack<IToken> stack = tokens
            .Reverse()
            .ToStack();

        string TryGetString(string errorMsg) => stack.TryPop(out var result) && result is TokenValue ? (TokenValue)result : throw new SyntaxException(errorMsg);

        string ruleName = TryGetString("no rule name");
        TryGetString(syntaxErrorText).Assert<string, SyntaxException>(x => x == "=", x => $"{syntaxErrorText}, token={x}");

        stack.Count.Assert<int, SyntaxException>(x => x > 0, _ => "no rules specified");

        object test(object value, Func<IToken, IRule?> test) => value is IToken ? test((IToken)value) ?? value : value;

        IRule findRule(IToken token) => token
            .Func(x => test(x, y => IntRule.Match(y)))
            .Func(x => test(x, y => LiteralRule.Match(y)))
            .Func(x => test(x, y => DataTypeRule.Match(y))) switch
        {
            IRule v => v,
            _ => throw new SyntaxException($"syntax error, invalid '{token}'"),
        };

        var rules = stack.Reverse().ToArray()
            .Select(x => findRule(x))
            .ToArray();

        return new GrammarRule(ruleName, rules);
    }
}
