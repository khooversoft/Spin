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
using Toolbox.Types;

namespace Toolbox.Parser.Grammar;

public class RuleBuilder
{
    private static IReadOnlyList<Func<Cursor<IToken>, IReadOnlyList<IRule>?>> _ruleFunctions = new Func<Cursor<IToken>, IReadOnlyList<IRule>?>[]
    {
        c => IntRule.Match(c),
    };

    public static IReadOnlyList<GrammarRule> Build(IEnumerable<string> lines)
    {
        const string syntaxErrorText = "syntax error";

        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .Add("=", "|")
            .Parse(lines.Join(" "));

        Cursor<IToken> tokenCursor = tokens
            .Where(x => x is TokenValue token && token.Value != " ")
            .ToArray()
            .ToCursor();

        string ruleName = tryGetString("no rule name");
        tryGetString(syntaxErrorText).Assert<string, SyntaxException>(x => x == "=", x => $"{syntaxErrorText}, token={x}");

        tokenCursor.List.Count.Assert<int, SyntaxException>(x => x > 0, _ => "no rules specified");

        List<IRule> rulesList = new();




        //var rules = stack.Reverse().ToArray()
        //    .Select(x => findRule(x))
        //    .ToArray();

        return null; // new GrammarRule(ruleName, rulesList);


        //  ///////////////////////////////////////////////////////////////////////////////////////

        string tryGetString(string errorMsg) => tokenCursor.TryNextValue(out var result) && result is TokenValue ? (TokenValue)result : throw new SyntaxException(errorMsg);

        object test(object value, Func<IToken, IRule?> test) => value is IToken ? test((IToken)value) ?? value : value;

        //IRule findRule(IToken token) => token
        //    .Func(x => test(x, y => IntRule.Match(y)))
        //    .Func(x => test(x, y => LiteralRule.Match(y)))
        //    .Func(x => test(x, y => DataTypeRule.Match(y))) switch
        //{
        //    IRule v => v,
        //    _ => throw new SyntaxException($"syntax error, invalid '{token}'"),
        //};
    }

    //internal static void ParseRules(Cursor<IToken> cursor, IList<IRule> rulesList)
    //{
    //    int currentIndex = cursor.Index;

    //    while (!cursor.IsCursorAtEnd)
    //    {
    //        var result = _ruleFunctions
    //            .Select(x => x(cursor))
    //            .FirstOrDefault() switch
    //        {
    //            null => throw new SyntaxException($"Syntax error - cannot find rule pattern: {cursor}")
    //        };

    //    }
    //}
}
