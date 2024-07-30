using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

public class SyntaxParser
{
    private readonly MetaSyntaxRoot _syntaxRoot;
    private readonly IReadOnlyList<string> _parseTokens;

    public SyntaxParser(MetaSyntaxRoot syntaxRoot)
    {
        _syntaxRoot = syntaxRoot.NotNull();
        _parseTokens = _syntaxRoot.GetParseTokens();
    }

    public Option Parse(string rawData)
    {
        var tokens = new StringTokenizer()
            .UseSingleQuote()
            .UseDoubleQuote()
            .UseCollapseWhitespace()
            .Add(_parseTokens)
            .SetFilter(x => x.Value.IsNotEmpty())
            .Parse(rawData);

        var pContext = new SyntaxParserContext(tokens);

        while (pContext.TokensCursor.TryPeekValue(out var _))
        {

        }

        Option status = pContext.TokensCursor.TryPeekValue(out var _) switch
        {
            false => StatusCode.OK,
            true => (StatusCode.BadRequest, pContext.ErrorMessage("End of tokens reached before completing")),
        };

        return status;
    }
}


public static class MetaSyntaxRootTool
{
    public static IReadOnlyList<string> GetParseTokens(this MetaSyntaxRoot syntaxRoot) => syntaxRoot.Rule
        .GetAll<TerminalSymbol>()
        .Where(x => x.Type == TerminalType.Token)
        .Select(x => x.Text)
        .ToImmutableArray();
}

