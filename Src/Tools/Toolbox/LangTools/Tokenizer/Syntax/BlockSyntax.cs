namespace Toolbox.LangTools;

/// <summary>
/// Block syntax handles block sub data that is marked by a delimiter such as quote ("), single (').
/// The block start and ending signal must be the same.
/// Handles escaping characters with "\" (back slash)s
/// </summary>
public class BlockSyntax : ITokenSyntax
{
    public BlockSyntax(char blockSignal = '"')
    {
        StartSignal = blockSignal;
        StopSignal = blockSignal;
        Priority = 1;
    }

    public BlockSyntax(char startSignal, char stopSignal)
    {
        StartSignal = startSignal;
        StopSignal = stopSignal;
        Priority = 1;
    }

    public char StartSignal { get; }
    public char StopSignal { get; }

    public int Priority { get; }

    public int? Match(ReadOnlySpan<char> span)
    {
        if (span.Length == 0) return null;
        if (span[0] != StartSignal) return null;

        bool isEscape = false;
        for (int index = 1; index < span.Length; index++)
        {
            if (isEscape)
            {
                isEscape = false;
                //if (span[index] != StopSignal) throw new ArgumentException($"Invalid escape sequence for token={span[index]}, stopSignal={StopSignal}");
                continue;
            }

            if (span[index] == '\\')
            {
                isEscape = true;
                continue;
            }

            if (span[index] == StopSignal) return index + 1;
        }

        throw new ArgumentException($"Missing ending signal={StopSignal}");
    }

    public IToken CreateToken(ReadOnlySpan<char> span, int index)
    {
        string value = span.ToString();
        return new BlockToken(value, StartSignal, StopSignal, index);
    }
}