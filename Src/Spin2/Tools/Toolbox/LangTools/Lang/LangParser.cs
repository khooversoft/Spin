﻿using Toolbox.Extensions;
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
            result = (StatusCode.BadRequest, "Syntax error, input tokens not completed");
            pContext.Log(TraceType.Error, nameof(Parse), result);
        }

        var response = new LangResult
        {
            StatusCode = result.StatusCode,
            Error = result.Error,
            RawData = rawData,
            LangNodes = result.IsOk() ? result.Return() : null,
            Traces = pContext.Trace,
        };

        return response;
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

    public static Option<LangNodes> MatchSyntaxSegement(this ILangRoot langRoot, string syntaxName, LangParserContext pContext)
    {
        var syntaxCursor = langRoot.CreateCursor();
        using var pScope = pContext.PushWithScope(langRoot);
        var nodes = new LangNodes();

        pContext.Log(TraceType.Start, syntaxName, langRoot.Name);

        while (syntaxCursor.TryNextValue(out var syntax))
        {
            Option<LangNodes> state = syntax.Process(pContext, syntaxCursor);
            pContext.Log(state.IsError() ? TraceType.Error : TraceType.Process, syntaxName, state, syntax.Name);
            if (state.IsError()) return state;

            if (state.IsOk()) nodes += state.Return();
        }

        if (syntaxCursor.TryNextValue(out _))
        {
            pContext.Log(TraceType.Error, syntaxName, langRoot.Name);
            return (StatusCode.BadRequest, "Syntax error, end");
        }

        pScope.Cancel();
        pContext.Log(TraceType.Ok, syntaxName, langRoot.Name);
        return nodes;
    }
}