using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.LangTools;

public record MetaSyntaxRoot
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public ProductionRule Rule { get; init; } = null!;
    public IReadOnlyList<string> Delimiters { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ReserveWords { get; init; } = Array.Empty<string>();

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
        Delimiters = subject.Delimiters.ToImmutableArray(),
        ReserveWords = subject.ReserveWords.ToImmutableArray(),
    };

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