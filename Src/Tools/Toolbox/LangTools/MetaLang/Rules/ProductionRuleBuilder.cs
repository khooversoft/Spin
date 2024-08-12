using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.LangTools;

public class ProductionRuleBuilder
{
    public string Name { get; init; } = null!;
    public ProductionRuleType Type { get; init; } = ProductionRuleType.Root;
    public EvaluationType EvaluationType { get; set; } = EvaluationType.Sequence;
    public Sequence<IMetaSyntax> Children { get; init; } = new Sequence<IMetaSyntax>();
    public int? Index { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}


public static class ProductionRuleBuilderExtensions
{
    public static ProductionRule ConvertTo(this ProductionRuleBuilder subject) => new ProductionRule
    {
        Name = subject.Name,
        Type = subject.Type,
        EvaluationType = subject.EvaluationType,
        Children = subject.Children.ToImmutableArray(),
        Index = subject.Index,
        Tags = subject.Tags.ToImmutableArray(),
    };
}
