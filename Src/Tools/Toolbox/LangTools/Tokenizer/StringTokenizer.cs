using Toolbox.Data;
using Toolbox.Tools;

namespace Toolbox.LangTools;

/// <summary>
/// String tokenizer, parses string for values and tokens based on token syntax
/// </summary>
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

    public IReadOnlyList<IToken> Parse(params string[] sources) => Parse(string.Join(string.Empty, sources));
    public IReadOnlyList<IToken> Parse(IEnumerable<string> sources) => Parse(string.Join(string.Empty, sources));


    public IReadOnlyList<IToken> Parse(string? source)
    {
        var tokenList = new List<IToken>();

        if (source == null || source == string.Empty) return tokenList;

        ITokenSyntax[] syntaxRules = _syntaxList
            .OrderByDescending(x => x.Priority)
            .ToArray();

        int? dataStart = null;

        source = DataTool.Filter(source, DataTool.IsAsciiRange, Convert);
        ReadOnlySpan<char> span = source.AsSpan();

        for (int index = 0; index < span.Length; index++)
        {
            int? matchLength = null;

            for (int syntaxIndex = 0; syntaxIndex < syntaxRules.Length; syntaxIndex++)
            {
                matchLength = syntaxRules[syntaxIndex].Match(span.Slice(index));
                if (matchLength == null) continue;

                if (dataStart != null)
                {
                    string dataValue = span
                        .Slice((int)dataStart, index - (int)dataStart)
                        .ToString();

                    tokenList.Add(new TokenValue(dataValue));
                    dataStart = null;
                }

                IToken token = syntaxRules[syntaxIndex].CreateToken(span.Slice(index, (int)matchLength));
                tokenList.Add(token);
                break;
            }

            if (matchLength == null)
            {
                dataStart ??= index;
                continue;
            }

            index += (int)matchLength - 1;
        }

        if (dataStart != null)
        {
            string dataValue = span
                .Slice((int)dataStart, span.Length - (int)dataStart)
                .ToString();

            tokenList.Add(new TokenValue(dataValue));
        }

        if (_filter != null) tokenList = tokenList.Where(x => _filter(x)).ToList();

        return tokenList;
    }

    private char Convert(char chr) => chr == 0x60 ? '`' : chr;
}