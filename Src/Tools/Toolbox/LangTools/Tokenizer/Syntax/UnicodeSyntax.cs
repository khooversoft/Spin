using Toolbox.Data;

namespace Toolbox.LangTools;

public class UnicodeSyntax : ITokenSyntax
{
    private const int _length = 6;
    public UnicodeSyntax() => Priority = 2;

    public int Priority { get; }

    public int? Match(ReadOnlySpan<char> span)
    {
        if (span.Length < _length) return null;

        ReadOnlySpan<char> slice = span.Slice(0, _length);

        int? index = DataTool.Unicode.CheckCommon(slice);
        if (index != null) return index;

        index = DataTool.Unicode.CheckStandard(slice);
        if (index != null) return index;

        return null;
    }

    public IToken CreateToken(ReadOnlySpan<char> span)
    {
        string value = span.ToString();
        return new TokenValue(value) { IsSyntaxToken = true, TokenType = TokenType.Unicode };
    }
}