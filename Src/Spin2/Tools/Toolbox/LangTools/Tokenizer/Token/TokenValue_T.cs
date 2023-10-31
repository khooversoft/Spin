//namespace Toolbox.Tokenizer.Token;

///// <summary>
///// Token value with token type
///// </summary>
///// <typeparam name="T">type of token</typeparam>
//public struct TokenValue<T> : IToken where T : Enum
//{
//    public TokenValue(string value, T tokenType)
//    {
//        Value = value;
//        TokenType = tokenType;
//    }

//    public string Value { get; }

//    public T TokenType { get; }

//    public override string ToString() => Value;

//    public override bool Equals(object? obj) => (obj is TokenValue<T> value) && value.Value == Value && value.TokenType.CompareTo(TokenType) == 0;

//    public override int GetHashCode() => Value.GetHashCode();

//    public static bool operator ==(TokenValue<T> left, TokenValue<T> right) => left.Equals(right);

//    public static bool operator !=(TokenValue<T> left, TokenValue<T> right) => !(left == right);
//}
