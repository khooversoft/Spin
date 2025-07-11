using System.Diagnostics;

namespace Toolbox.LangTools;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record VirtualTerminalSymbol : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
    public int? Index { get; init; }

    public bool Equals(VirtualTerminalSymbol? obj)
    {
        bool result = obj is VirtualTerminalSymbol subject &&
            Name == subject.Name &&
            Text == subject.Text;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Text, Index);
    public override string ToString() => $"VirtualTerminalSymbol [ Name={Name}, Index={Index} ]";
    public string GetDebuggerDisplay() => $"VirtualTerminalSymbol: Name={Name}, Text={Text}, Index={Index}";
}
