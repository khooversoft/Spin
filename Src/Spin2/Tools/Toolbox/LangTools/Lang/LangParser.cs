using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public static class LangParser
{
    public static Option<LangNodes> Parse(this ILangRoot langRoot, string rawData)
    {
        var tokens = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .SetFilter(x => x.Value.IsNotEmpty())
            .Add(langRoot.GetSyntaxTokens())
            .Parse(rawData);

        var pContext = new LangParserContext(langRoot, tokens);

        var result = langRoot.Process(pContext);

        if (pContext.TokensCursor.TryPeekValue(out var _)) return (StatusCode.BadRequest, "Syntax error, input tokens not completed");
        return result;
    }

    public static string[] GetSyntaxTokens(this ILangRoot langRoot)
    {
        var stack = new Stack<ILangRoot>(new[] { langRoot });
        var list = new List<string>();

        while (stack.TryPop(out var root))
        {
            root.Children.OfType<LsToken>().ForEach(x => list.Add(x.Symbol));
            root.Children.OfType<LsGroup>().ForEach(x => list.AddRange(new[] { x.StartToken, x.EndToken }));
            root.Children.OfType<ILangRoot>().ForEach(x => stack.Push(x));
        }

        return list.ToArray();
    }

    public static Option<LangNodes> MatchSyntaxSegement(this ILangRoot langRoot, LangParserContext pContext)
    {
        var syntaxCursor = langRoot.CreateCursor();
        using var pScope = pContext.PushWithScope(langRoot);
        var nodes = new LangNodes();

        while (syntaxCursor.TryNextValue(out var syntax))
        {
            Option<LangNodes> state = syntax.Process(pContext, syntaxCursor);
            if (state.IsError()) return state;

            if (state.IsOk()) nodes += state.Return();
        }

        if (syntaxCursor.TryNextValue(out _)) return (StatusCode.BadRequest, "Syntax error, end");

        pScope.Cancel();
        return nodes;
    }
}
