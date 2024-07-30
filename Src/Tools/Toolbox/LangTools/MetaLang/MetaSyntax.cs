using System.Collections.Immutable;
using System.Diagnostics;
using Toolbox.Extensions;

namespace Toolbox.LangTools;

public interface IMetaSyntax
{
    string Name { get; }
    public int? Index { get; }
}

public enum TerminalType
{
    Token,
    String,
    Regex,
}

[DebuggerDisplay("TerminalSymbol: Name={Name}, Text={Text}, Type={Type}, Index={Index}, Tags={TagStrings}")]
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
    public override string ToString() => $"TerminalSymbol [ Name={Name}, Text={Text}, Type={Type}, Index={Index}, Tags={Tags.Join(";")} ]";
}

[DebuggerDisplay("VirtualTerminalSymbol: Name={Name}, Text={Text}, Index={Index}")]
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
}
