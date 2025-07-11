using System.Diagnostics;

namespace Toolbox.LangTools;

/// <summary>
/// Block token that has been extracted from the data.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct BlockToken : IToken
{
    public BlockToken(string value, char startSignal, char stopSignal, int index)
    {
        StartSignal = startSignal;
        StopSignal = stopSignal;

        if (value.Length < 2) throw new ArgumentException("Length to small for quoted data");
        if (value[0] != StartSignal) throw new ArgumentException("Start signal does not match");
        if (value[^1] != StopSignal) throw new ArgumentException("Stop signal does not match");

        Value = value.Substring(1, value.Length - 2);
        Index = index;
    }

    public char StartSignal { get; }
    public char StopSignal { get; }

    public string Value { get; }
    public TokenType TokenType { get; } = TokenType.Block;
    public int? Index { get; }

    public override string ToString() => Value;
    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => obj is BlockToken value &&
        value.Value == Value &&
        value.StartSignal == StartSignal &&
        value.StopSignal == StopSignal;

    public static bool operator ==(BlockToken left, BlockToken right) => left.Equals(right);
    public static bool operator !=(BlockToken left, BlockToken right) => !(left == right);

    public string GetDebuggerDisplay() => $"BlockToken: TokenType={TokenType.ToString()}, Token={Value}, StartSignal={StartSignal}, StopSignal={StopSignal}2, Index={Index}";

}