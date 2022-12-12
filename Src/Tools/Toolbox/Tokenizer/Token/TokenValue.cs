using System;
using System.Collections.Generic;
using System.Text;
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

        public override bool Equals(object? obj) => obj is TokenValue value && Value == value.Value;

        public override int GetHashCode() => HashCode.Combine(Value);

        public override string ToString() => Value;

        public static bool operator ==(TokenValue left, TokenValue right) => left.Equals(right);

        public static bool operator !=(TokenValue left, TokenValue right) => !(left == right);

        public static implicit operator string(TokenValue tokenValue) => tokenValue.Value;
    }
}
