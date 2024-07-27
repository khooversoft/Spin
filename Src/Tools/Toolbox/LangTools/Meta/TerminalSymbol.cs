namespace Toolbox.LangTools;

public record TerminalSymbol : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
    public bool Regex { get; init; }
}

public record VirtualTerminalSymbol : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
}
