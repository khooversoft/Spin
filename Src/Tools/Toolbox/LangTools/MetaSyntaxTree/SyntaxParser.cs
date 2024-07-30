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
            var status2 = ProcessRules(pContext);
        }

        Option status = pContext.TokensCursor.TryPeekValue(out var _) switch
        {
            false => StatusCode.OK,
            true => (StatusCode.BadRequest, pContext.ErrorMessage("End of tokens reached before completing")),
        };

        return status;
    }

    private Option ProcessRules(SyntaxParserContext pContext)
    {
        foreach (var child in _syntaxRoot.Rule.Children)
        {
            Option status = child switch
            {
                TerminalSymbol terminal => ProcessTerminal(pContext, terminal),
                ProductionRule productionRule => ProcessRules(pContext, productionRule),

                _ => (StatusCode.BadRequest, pContext.ErrorMessage("Unknown token")),
            };

            if (!status.IsOk()) return status;
        }

        return StatusCode.OK;
    }

    private Option ProcessTerminal(SyntaxParserContext pContext, TerminalSymbol terminal)
    {
        using var scope = pContext.PushWithScope();

        if( pContext.TokensCursor.Current.Value != terminal.Text) return (StatusCode.BadRequest, pContext.ErrorMessage($"Expected '{terminal.Text}'"));
        pContext.Pairs.Add(new SyntaxPair { MetaSyntax = terminal, Text = terminal.Text });

        return StatusCode.OK;
    }

    private Option ProcessRules(SyntaxParserContext pContext, ProductionRule productionRule)
    {
        throw new NotImplementedException();
    }
}

public record SyntaxPair
{
    public IMetaSyntax MetaSyntax { get; init; } = null!;
    public string Text { get; init; } = null!;
}


public static class MetaSyntaxRootTool
{
    public static IReadOnlyList<string> GetParseTokens(this MetaSyntaxRoot syntaxRoot) => syntaxRoot.Rule
        .GetAll<TerminalSymbol>()
        .Where(x => x.Type == TerminalType.Token)
        .Select(x => x.Text)
        .ToImmutableArray();
}

