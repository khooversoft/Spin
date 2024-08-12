using System.Diagnostics;
using Toolbox.Extensions;

namespace Toolbox.LangTools;

public interface IMetaSyntax
{
    string Name { get; }
    int? Index { get; }
    string GetDebuggerDisplay();
}

public enum TerminalType
{
    Token,
    String,
    Regex,
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record TerminalSymbol : IMetaSyntax, IEquatable<TerminalSymbol>
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
    public TerminalType Type { get; init; } = TerminalType.Token;
    public int? Index { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public string TagStrings => Tags.Join(",");

    public bool Equals(TerminalSymbol? obj)
    {
        bool result = obj is TerminalSymbol subject &&
            Name == subject.Name &&
            Text == subject.Text &&
            Type == subject.Type &&
            Enumerable.SequenceEqual(Tags, subject.Tags);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Text, Type, Index);
    public override string ToString() => $"TerminalSymbol [ Name={Name}, Text={Text}, Type={Type.ToString()}, Index={Index}, Tags={Tags.Join(";")} ]";
    public string GetDebuggerDisplay() => $"TerminalSymbol: Name={Name}, Text={Text}, Type={Type.ToString()}, Index={Index}, Tags={TagStrings}";
}
