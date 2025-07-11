using System.Diagnostics;

namespace Toolbox.LangTools;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record SyntaxPair : ISyntaxTree
{
    public IToken Token { get; init; } = null!;
    public string MetaSyntaxName { get; init; } = null!;

    public string GetDebuggerDisplay() => $"Token={Token.Value}, MetaSyntaxName={MetaSyntaxName}";

    public bool Equals(SyntaxPair? obj)
    {
        bool result = obj is SyntaxPair subject &&
            Token.Equals(subject.Token) &&
            MetaSyntaxName.Equals(subject.MetaSyntaxName);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Token, MetaSyntaxName);
}
