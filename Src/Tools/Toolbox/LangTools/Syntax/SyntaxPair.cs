using System.Diagnostics;

namespace Toolbox.LangTools;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record SyntaxPair : ISyntaxTree
{
    public IToken Token { get; init; } = null!;
    public string Name { get; init; } = null!;

    public string GetDebuggerDisplay() => $"Token={Token.Value}, MetaSyntaxName={Name}";

    public bool Equals(SyntaxPair? obj)
    {
        bool result = obj is SyntaxPair subject &&
            Token.Equals(subject.Token) &&
            Name.Equals(subject.Name);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Token, Name);
}
