namespace Toolbox.LangTools;

/// <summary>
/// Block token that has been extracted from the data.
/// </summary>
public struct BlockToken : IToken
{
    public BlockToken(string value, char startSignal, char stopSignal)
    {
        StartSignal = startSignal;
        StopSignal = stopSignal;

        if (value.Length < 2) throw new ArgumentException("Length to small for quoted data");
        if (value[0] != StartSignal) throw new ArgumentException("Start signal does not match");
        if (value[^1] != StopSignal) throw new ArgumentException("Stop signal does not match");

        Value = value.Substring(1, value.Length - 2);
    }

    public char StartSignal { get; }
    public char StopSignal { get; }

    public string Value { get; }
    public TokenType TokenType { get; } = TokenType.Block;

    public override string ToString() => Value;
    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => obj is BlockToken value &&
        value.Value == Value &&
        value.StartSignal == StartSignal &&
        value.StopSignal == StopSignal;

    public static bool operator ==(BlockToken left, BlockToken right) => left.Equals(right);
    public static bool operator !=(BlockToken left, BlockToken right) => !(left == right);
}