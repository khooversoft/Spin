using System.Diagnostics;

namespace Toolbox.LangTools;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record ProductionRuleReference : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public string ReferenceSyntax { get; init; } = null!;
    public int? Index { get; init; }

    public bool Equals(ProductionRuleReference? obj) => obj is ProductionRuleReference subject &&
        Name == subject.Name &&
        ReferenceSyntax == subject.ReferenceSyntax;

    public override int GetHashCode() => HashCode.Combine(Name);
    public override string ToString() => $"ProductionRuleReference [ Name={Name}, Index={Index} ]";
    public string GetDebuggerDisplay() => $"ProductionRuleReference: Name={Name}, ReferenceSyntax={ReferenceSyntax}, Index={Index}";
}
