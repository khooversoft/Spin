using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;

public static class LangParser
{
    public static LangResult Parse(this ILangRoot langRoot, string rawData)
    {
        var tokens = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .SetFilter(x => x.Value.IsNotEmpty())
            .Add(langRoot.GetSyntaxTokens())
            .Parse(rawData);

        var pContext = new LangParserContext(langRoot, tokens);

        Option<LangNodes> result = langRoot.Process(pContext);

        if (result.IsOk() && pContext.TokensCursor.TryPeekValue(out var _))
        {
            string left = pContext.TokensCursor.List
                .Skip(pContext.TokensCursor.Index)
                .Select(x => x.Value)
                .Join(" ");

            Debugger.Break();

            result = (StatusCode.BadRequest, $"Syntax error, input tokens not completed, rawData='{rawData}' left tokens='{left}'.");
        }

        var response = new LangResult
        {
            StatusCode = result.StatusCode,
            Error = result.Error,
            RawData = rawData,
            LangNodes = result.IsOk() ? result.Return() : null,
            MaxTokens = tokens.Take(pContext.TokensCursor.MaxIndex).Select(x => x.Value).Join(" "),
        };

        return response;
    }

    public static string[] GetSyntaxTokens(this ILangRoot langRoot)
    {
        var stack = new Stack<ILangRoot>(new[] { langRoot });
        var list = new List<string>();

        while (stack.TryPop(out var root))
        {
            root.Children.OfType<LsToken>().ForEach(x => list.Add(x.Token));
            root.Children.OfType<LsGroup>().ForEach(x => list.AddRange(new[] { x.StartToken, x.EndToken }));
            root.Children.OfType<ILangRoot>().ForEach(x => stack.Push(x));
        }

        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static Option<LangNodes> MatchSyntaxSegement(this ILangRoot langRoot, string syntaxName, LangParserContext pContext)
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

        if (syntaxCursor.TryNextValue(out _))
        {
            return (StatusCode.BadRequest, "Syntax error, end");
        }

        pScope.Cancel();
        return nodes;
    }
}
