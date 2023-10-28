using Toolbox.Tools;

namespace Toolbox.Tokenizer.Token
{
    /// <summary>
    /// Token value extracted from data
    /// </summary>
    public readonly struct TokenValue : IToken
    {
        public TokenValue(string value) => Value = value.NotNull();

        public string Value { get; init; }
        public bool IsSyntaxToken { get; init; }

        public override bool Equals(object? obj) => obj is TokenValue value && Value == value.Value;
        public override int GetHashCode() => HashCode.Combine(Value);
        public override string ToString() => Value;

        public static bool operator ==(TokenValue left, TokenValue right) => left.Equals(right);
        public static bool operator !=(TokenValue left, TokenValue right) => !(left == right);

        public static explicit operator string(TokenValue tokenValue) => tokenValue.Value;
    }
}
