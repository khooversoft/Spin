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

        while (pContext.TokensCursor.TryNextValue(out var _))
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
        var rules = _syntaxRoot.Rule.Children.OfType<ProductionRule>();

        foreach (var child in rules)
        {
            Option status = ProcessRule(pContext, child);

            if (status.IsNotFound()) continue;
            if (!status.IsError()) return status;
        }

        return StatusCode.OK;
    }
    private Option ProcessRule(SyntaxParserContext pContext, ProductionRule productionRule)
    {
        using var scope = pContext.PushWithScope();
        var ruleCursor = productionRule.Children.ToCursor();

        while (pContext.TokensCursor.TryNextValue(out var token))
        {


        }

        return StatusCode.OK;
    }

    private Option ProcessTerminal(SyntaxParserContext pContext, TerminalSymbol terminal)
    {
        switch (pContext.TokensCursor.Current)
        {
            case var v when v.TokenType == TokenType.Token && v.Value == terminal.Text:
                pContext.Pairs.Add(new SyntaxPair { MetaSyntax = terminal, Text = terminal.Text });
                return StatusCode.OK;

            default:
                return StatusCode.NotFound;
        };
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

