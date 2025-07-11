using System.Diagnostics;
using Toolbox.Tools;

namespace Toolbox.LangTools;

/// <summary>
/// Token value extracted from data
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct TokenValue : IToken
{
    public TokenValue(string value) => Value = value.NotNull();
    public TokenValue(string value, int index) => (Value, Index) = (value.NotNull(), index);

    public string Value { get; init; }
    public int? Index { get; init; }
    public bool IsSyntaxToken { get; init; }
    public TokenType TokenType { get; init; } = TokenType.Token;


    public override bool Equals(object? obj) => obj is TokenValue value && Value == value.Value;
    public override int GetHashCode() => HashCode.Combine(Value);
    public override string ToString() => Value;

    public static bool operator ==(TokenValue left, TokenValue right) => left.Equals(right);
    public static bool operator !=(TokenValue left, TokenValue right) => !(left == right);

    public static explicit operator string(TokenValue tokenValue) => tokenValue.Value;

    public string GetDebuggerDisplay() => $"TokenValue: Type={TokenType.ToString()}, Value={Value}, Index={Index}, IsSyntaxToken={IsSyntaxToken}";
}

