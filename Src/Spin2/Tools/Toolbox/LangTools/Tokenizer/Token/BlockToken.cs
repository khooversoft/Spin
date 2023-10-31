namespace Toolbox.LangTools;

/// <summary>
/// Block token that has been extracted from the data.
/// </summary>
public struct BlockToken : IToken
{
    public BlockToken(string value)
    {
        BlockSignal = value[0];

        if (value.Length < 2) throw new ArgumentException("Length to small for quoted data");
        if (value[^1] != BlockSignal) throw new ArgumentException("Ending quote does not match beginning");

        Value = value.Substring(1, value.Length - 2);
    }

    public char BlockSignal { get; }
    public string Value { get; }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is BlockToken value && value.Value == Value && value.BlockSignal == BlockSignal;
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(BlockToken left, BlockToken right) => left.Equals(right);
    public static bool operator !=(BlockToken left, BlockToken right) => !(left == right);
}