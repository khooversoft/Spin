using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Parser.Grammar;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Types;
using Toolbox.Types.Structure;

namespace Toolbox.Parser.Syntax;

public class SyntaxTreeBuilder
{
    public Tree? Build(string line, Tree tree)
    {
        Cursor<IToken> tokens = Parse(line);

        var result = tree
            .OfType<RuleNode>()
            .Select(x => ProcessRules(x, tokens))
            .SkipWhile(x => x == null)
            .FirstOrDefault();

        if (result == null) return null;

        return new Tree() + result;
    }

    private static Cursor<IToken> Parse(string line) => new StringTokenizer()
        .UseCollapseWhitespace()
        .UseDoubleQuote()
        .UseSingleQuote()
        .Add("=", ",", ";", "|", "[", "]", "{", "}", "(", ")", "-")
        .Parse(line)
        .Where(x => x is not TokenValue v || !v.Value.IsEmpty())
        .Select(x => x switch
        {
            TokenValue v => v with { Value = v.Value.Trim() },
            _ => x,
        })
        .ToArray()
        .ToCursor();

    public static IRuleValue? ProcessRules(IRule rule, Cursor<IToken> tokens)
    {
        int cursorSave = tokens.Index;
        Sequence<IRuleValue?> values = new();

        IRuleValue? ruleMatch = ProcessRule(rule, tokens);
        if (ruleMatch == null) return null;


        int count = rule.OfType<IRule>().Count();

        IRuleValue[] result = rule.OfType<IRule>()
            .Select(x => tokens.IsCursorAtEnd switch
            {
                true => null,
                false => ProcessRule(x, tokens),
            })
            .TakeWhile(x => x != null)
            .OfType<IRuleValue>()
            .ToArray();

        if (result.Length != count)
        {
            tokens.Index = cursorSave;
            return null;
        }

        return ruleMatch + result.OfType<IRuleValue>();
    }

    private static IRuleValue? ProcessRule(IRule rule, Cursor<IToken> tokens)
    {
        var result = rule switch
        {
            IRuleSingle v => v.Match(tokens.Current),
            IRuleFunction v => v.Match(tokens),
            _ => throw new UnreachableException($"token.Type={rule.GetType().FullName}"),
        };

        if (result != null) tokens.Index++;
        return result;
    }
}
