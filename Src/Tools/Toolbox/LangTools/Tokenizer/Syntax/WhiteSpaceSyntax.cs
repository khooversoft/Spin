namespace Toolbox.LangTools;

/// <summary>
/// Defines a whit space token.  Whitespace tokens are any character 32 or less
/// and are compressed to a space " " token.
/// </summary>
public class WhiteSpaceSyntax : ITokenSyntax
{
    public int Priority { get; }

    public IToken CreateToken(ReadOnlySpan<char> span, int index) => new TokenValue(" ", index);

    public int? Match(ReadOnlySpan<char> span)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (char.IsWhiteSpace(span[i]))
            {
                continue;
            }

            return i > 0 ? i : (int?)null;
        }

        return span.Length;
    }
}
