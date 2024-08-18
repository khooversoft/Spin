namespace Toolbox.LangTools;

public sealed record TerminalNode : ISyntaxNode
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
    public int? Index { get; init; }

    public bool Equals(TerminalNode? obj)
    {
        bool result = obj is TerminalNode subject &&
            Name == subject.Name &&
            Text == subject.Text;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Text, Index);
    public override string ToString() => $"TerminalNode [ Name={Name}, Text={Text}, Index={Index} ]";
}