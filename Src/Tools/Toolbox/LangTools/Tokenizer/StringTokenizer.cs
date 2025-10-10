using Toolbox.Tools;

namespace Toolbox.LangTools;

public class StringTokenizer
{
    private readonly List<ITokenSyntax> _syntaxList = new();
    private Func<IToken, bool>? _filter;

    public StringTokenizer UseCollapseWhitespace() { _syntaxList.Add(new WhiteSpaceSyntax()); return this; }
    public StringTokenizer UseSingleQuote() { _syntaxList.Add(new BlockSyntax('\'')); return this; }
    public StringTokenizer UseDoubleQuote() { _syntaxList.Add(new BlockSyntax('"')); return this; }
    public StringTokenizer UseUnicode() { _syntaxList.Add(new UnicodeSyntax()); return this; }

    public StringTokenizer AddBlock(char startSignal, char stopSignal) { _syntaxList.Add(new BlockSyntax(startSignal, stopSignal)); return this; }
    public StringTokenizer Add(params string[] tokens) { _syntaxList.AddRange(tokens.Select(x => (ITokenSyntax)new TokenSyntax(x))); return this; }
    public StringTokenizer Add(IEnumerable<string> tokens) { _syntaxList.AddRange(tokens.Select(x => (ITokenSyntax)new TokenSyntax(x))); return this; }
    public StringTokenizer Add(params ITokenSyntax[] tokenSyntaxes) { _syntaxList.AddRange(tokenSyntaxes); return this; }

    public StringTokenizer SetFilter(Func<IToken, bool> filter) { _filter = filter.NotNull(); return this; }

    public IReadOnlyList<IToken> Parse(string? source)
    {
        var rules = _syntaxList.OrderByDescending(x => x.Priority).ToArray();
        return TokenParserTool.Parse(source, rules, _filter);
    }

    public TokenParser Build() => new TokenParser(_syntaxList, _filter);
}

public class TokenParser
{
    private readonly ITokenSyntax[] _syntaxRules;
    private readonly Func<IToken, bool>? _filter;

    public TokenParser(IEnumerable<ITokenSyntax> syntaxRules, Func<IToken, bool>? filter)
    {
        _syntaxRules = syntaxRules.NotNull().OrderByDescending(x => x.Priority).ToArray();
        _filter = filter;
    }

    public IReadOnlyList<IToken> Parse(string? source) => TokenParserTool.Parse(source, _syntaxRules, _filter);
}

public static class TokenParserTool
{
    public static IReadOnlyList<IToken> Parse(string? source, IReadOnlyList<ITokenSyntax> syntaxRules, Func<IToken, bool>? filterToken)
    {
        if (string.IsNullOrEmpty(source)) return Array.Empty<IToken>();

        ReadOnlySpan<char> span = Clean(source);
        int dataStart = -1;

        // Heuristic capacity to reduce resizes; cap to keep allocation modest
        var tokenList = new List<IToken>(Math.Min(span.Length / 2, 1024));
        var rules = syntaxRules;
        int rulesLen = rules.Count;
        var filter = filterToken;

        // Hoist the common slice once per index
        for (int index = 0; index < span.Length; index++)
        {
            ReadOnlySpan<char> remaining = span.Slice(index);
            int matchLength = -1;

            for (int syntaxIndex = 0; syntaxIndex < rulesLen; syntaxIndex++)
            {
                int? m = rules[syntaxIndex].Match(remaining);
                if (m == null) continue;

                matchLength = m.Value;

                if (dataStart != -1)
                {
                    var dataToken = new TokenValue(span.Slice(dataStart, index - dataStart).ToString(), dataStart);
                    if (filter is null || filter(dataToken)) tokenList.Add(dataToken);
                    dataStart = -1;
                }

                IToken token = rules[syntaxIndex].CreateToken(remaining.Slice(0, matchLength), index);
                if (filter is null || filter(token)) tokenList.Add(token);
                break;
            }

            if (matchLength < 0) { if (dataStart == -1) dataStart = index; continue; }
            index += matchLength - 1;
        }

        if (dataStart != -1)
        {
            var dataToken = new TokenValue(span.Slice(dataStart, span.Length - dataStart).ToString(), dataStart);
            if (filter is null || filter(dataToken)) tokenList.Add(dataToken);
        }

        return tokenList;
    }

    private static ReadOnlySpan<char> Clean(string source)
    {
        const char graveAccent = (char)0x60;
        ReadOnlySpan<char> s = source.AsSpan();

        // Find first trigger: start of comment ("//") or any non-ASCII-printable char (we do not keep CR)
        int i = 0;
        for (; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '/') break; // comment start
            if (c < 32 || c >= 127) break;                              // non-printable (includes CR/LF/TAB) or non-ASCII
            if (c == graveAccent) break;
        }

        // Fast path: nothing to clean
        if (i == s.Length) return s;

        // Allocate once and copy the clean prefix (ASCII printable only)
        var dst = new char[s.Length];
        if (i > 0) s.Slice(0, i).CopyTo(dst);
        int w = i;

        // Copy remainder, skipping comments and non-printable/non-ASCII
        for (; i < s.Length; i++)
        {
            char c = s[i];

            // Strip comment: everything after "//" up to CR or end (CR itself is not kept)
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '/')
            {
                i += 2;
                while (i < s.Length)
                {
                    char d = s[i];
                    if (d == '\r') break; // stop at CR; do not emit CR
                    if (d == '\n') break; // stop at LF; do not emit LF
                    i++;
                }
                continue;
            }

            // Keep ASCII printable only (32..126); drop CR/LF/TAB/controls and >=127
            if (c >= 32 && c < 127)
            {
                dst[w++] = c == graveAccent ? '`' : c;
            }
        }

        return dst.AsSpan(0, w);
    }
}
