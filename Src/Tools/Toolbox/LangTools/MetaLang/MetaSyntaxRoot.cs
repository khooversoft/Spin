using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.LangTools;

public record MetaSyntaxRoot
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public ProductionRule Rule { get; init; } = null!;

    public IReadOnlyDictionary<string, IMetaSyntax> Nodes { get; init; } = ImmutableDictionary<string, IMetaSyntax>.Empty;
}


public static class MetaSyntaxRootExtensions
{
    public static MetaSyntaxRoot ConvertTo(this MetaParserContext subject, Option option) => new MetaSyntaxRoot
    {
        StatusCode = option.StatusCode,
        Error = option.Error,
        Rule = subject.RootRule.ConvertTo(),
        Nodes = subject.Nodes.ToImmutableDictionary(),
    };

    public static IReadOnlyList<string> GetParseTokens(this MetaSyntaxRoot syntaxRoot)
    {
        var terminalSymbols = syntaxRoot.Rule
            .GetAll<TerminalSymbol>()
            .Where(x => x.Type == TerminalType.Token)
            .Select(x => x.Text);

        var virtualSymbols = syntaxRoot.Rule
            .GetAll<VirtualTerminalSymbol>()
            .Select(x => x.Text);

        return terminalSymbols.Concat(virtualSymbols).Distinct().ToImmutableArray();
    }

    public static IReadOnlyList<ProductionRule> GetRootRules(this MetaSyntaxRoot syntaxRoot)
    {
        var ruleDependencies = syntaxRoot.Rule.Children.OfType<ProductionRule>()
            .SelectMany(x => syntaxRoot.GetDependencies(x), (o, i) => i.ReferenceSyntax)
            .Distinct()
            .ToHashSet();

        var ruleNoOneDependsOn = syntaxRoot.Rule.Children.OfType<ProductionRule>()
            .Where(x => !ruleDependencies.Contains(x.Name))
            .Select(x => syntaxRoot.Nodes[x.Name])
            .OfType<ProductionRule>()
            .ToImmutableArray();

        return ruleNoOneDependsOn;
    }

    public static IReadOnlyList<ProductionRuleReference> GetDependencies(this MetaSyntaxRoot syntaxRoot, ProductionRule rule) => rule
        .GetAll<ProductionRuleReference>()
        .ToImmutableArray();
}