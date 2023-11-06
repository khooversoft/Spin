using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public static class PatternParser
{
    public static Option<PatternNodes> Parse(this IPatternSyntax langRoot, string rawData)
    {
        var tokens = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .SetFilter(x => x.Value.IsNotEmpty())
            .Add(langRoot.GetSyntaxTokens())
            .Parse(rawData);

        var pContext = new PatternContext(tokens);
        pContext.Push(langRoot);

        Option<Sequence<IPatternSyntax>> resultOption = langRoot.MatchSyntaxSegement(pContext);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<PatternNodes>();

        if (resultOption.IsOk() && pContext.TokensCursor.TryPeekValue(out var _))
        {
            return (StatusCode.BadRequest, "Syntax error, input tokens not completed");
        }

        Sequence<IPatternSyntax> result = resultOption.Return();

        if (tokens.Count != result.Count) return (StatusCode.BadRequest, $"tokens.Count={tokens.Count} != result.Count={result.Count}");

        var merge = tokens.Zip(result).Select(x => new PatternNode(x.Second, x.First.Value));
        var patternNodes = new PatternNodes(merge);

        return patternNodes;
    }

    public static string[] GetSyntaxTokens(this IPatternSyntax langRoot)
    {
        var stack = new Stack<IPatternSyntax>(new[] { langRoot });
        var list = new List<string>();

        while (stack.TryPop(out var root))
        {
            switch (root)
            {
                case PmToken token:
                    list.Add(token.Symbol);
                    break;

                case IPatternBase<IPatternSyntax> collection:
                    collection.Children.ForEach(x => stack.Push(x));
                    break;
            }
        }

        return list.ToArray();
    }

    public static Option<Sequence<IPatternSyntax>> MatchSyntaxSegement(this IPatternSyntax langRoot, PatternContext pContext)
    {
        var nodes = new Sequence<IPatternSyntax>();

        while (pContext.Syntax.TryPeekValue(out var syntax))
        {
            Option<Sequence<IPatternSyntax>> state = syntax.Process(pContext);
            if (state.IsError()) return state;

            if (state.IsOk()) nodes += state.Return();
        }

        if (pContext.Syntax.TryNextValue(out _))
        {
            return (StatusCode.BadRequest, "Syntax error, end");
        }

        return nodes;
    }
}
