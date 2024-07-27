using Toolbox.Types;

namespace Toolbox.LangTools;

public enum ProductionRuleType
{
    Root,
    Group,
    Repeat,
    Optional,
}

public record ProductionRule : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public ProductionRuleType Type { get; init; }
    public Sequence<IMetaSyntax> Children { get; init; } = new Sequence<IMetaSyntax>();
}

public record ProductionRuleReference : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public IMetaSyntax ReferenceSyntax { get; init; } = null!;
}

