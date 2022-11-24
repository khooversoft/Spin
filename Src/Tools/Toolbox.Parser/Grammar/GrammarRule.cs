//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using Toolbox.Parser.Application;
//using Toolbox.Parser.Syntax;
//using Toolbox.Tokenizer.Token;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Parser.Grammar;

//public class GrammarRule
//{
//    private static IReadOnlyList<Func<Cursor<IToken>, IReadOnlyList<IRule>?>> _ruleFunctions = new Func<Cursor<IToken>, IReadOnlyList<IRule>?>[]
//    {
//        c => IntRule.Match(c),
//    };

//    public GrammarRule(string name, IEnumerable<IRule> rules)
//    {
//        Name = name.NotEmpty();
//        Rules = rules.NotNull().ToArray().Assert(x => x.Length > 0, "Empty list");
//    }

//    public string Name { get; }

//    public IReadOnlyList<IRule> Rules { get; }

//    public bool Match(Cursor<IToken> cursor, SyntaxTree syntaxTree)
//    {
//        cursor.NotNull();
//        syntaxTree.NotNull();
//        const string syntaxErrorText = "syntax error";

//        string ruleName = tryGetString("no rule name");
//        tryGetString(syntaxErrorText).Assert<string, SyntaxException>(x => x == "=", x => $"{syntaxErrorText}, token={x}");

//        cursor.List.Count.Assert<int, SyntaxException>(x => x > 0, _ => "no rules specified");

//        return true;

//        string tryGetString(string errorMsg) => cursor.TryNextValue(out var result) && result is TokenValue ?
//            (TokenValue)result :
//            throw new SyntaxException(errorMsg);

//        object test(object value, Func<IToken, IRule?> test) => value is IToken ? test((IToken)value) ?? value : value;
//    }
//}


//public static class GrammarRuleFactory
//{
//    public static GrammarRule? TryBuild(Cursor<IToken> cursor, SyntaxTree syntaxTree)
//    {
//        cursor.NotNull();
//        syntaxTree.NotNull();
//        const string syntaxErrorText = "syntax error";

//        string? ruleName = tryGetString("no rule name");
//        if (ruleName == null) return null;

//        if (tryGetString(syntaxErrorText) != "=") return null;
//        if (cursor.List.Count == 0) return null;


//        string? tryGetString(string errorMsg) => cursor.TryNextValue(out var result) && result is TokenValue ? (TokenValue)result : null;

//        object test(object value, Func<IToken, IRule?> test) => value is IToken ? test((IToken)value) ?? value : value;
//    }
//}
