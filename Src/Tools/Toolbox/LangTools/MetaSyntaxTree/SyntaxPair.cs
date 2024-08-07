using System.Diagnostics;

namespace Toolbox.LangTools;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed record SyntaxPair : ISyntaxTree
{
    public IToken Token { get; init; } = null!;
    public IMetaSyntax MetaSyntax { get; init; } = null!;

    public string GetDebuggerDisplay() => $"Token={Token.Value}, MetaSyntax={MetaSyntax.GetDebuggerDisplay()}";

    public bool Equals(SyntaxPair? obj)
    {
        bool result = obj is SyntaxPair subject &&
            Token.Equals(subject.Token) &&
            MetaSyntax.Equals(subject.MetaSyntax);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Token, MetaSyntax);
}
