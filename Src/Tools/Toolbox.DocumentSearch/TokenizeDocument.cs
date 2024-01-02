using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch;

public class TokenizeDocument
{
    private readonly WordTokenList _wordWeightList;

    public TokenizeDocument() => _wordWeightList = new WordTokenList();
    public TokenizeDocument(WordTokenList wordWeightList) => _wordWeightList = wordWeightList.NotNull();

    public IReadOnlyList<WordToken> Parse(string subject, IEnumerable<string>? tags = null)
    {
        if (subject.IsEmpty()) return Array.Empty<WordToken>();

        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .Add(_parseTokens)
            .AddBlock('<', '>')
            .SetFilter(x => IsValid(x))
            .Parse(subject);

        var wordTokens = tokens
            .Select(x => _wordWeightList.Dictionary.TryGetValue(x.Value, out var wordWeight) switch
            {
                false => new WordToken(x.Value, 0),
                true => wordWeight,
            })
            .Concat((tags ?? Array.Empty<string>()).Select(x => new WordToken(x, 3)))
            .Where(x => x.Weight >= 0)
            .GroupBy(x => x.Word)
            .Select(x => new WordToken(x.Key, x.Max(x => x.Weight)))
            .ToArray();

        return wordTokens;
    }

    private bool IsValid(IToken token)
    {
        switch (token)
        {
            case BlockToken: return false;
            case TokenValue v when v.IsSyntaxToken: return false;

            case IToken v:
                if (v.Value.IsEmpty()) return false;
                if (v.Value.Length == 1) return false;
                if (_stopWords.Contains(v.Value)) return false;
                return true;

            default: return false;
        }
    }

    private static FrozenSet<string> _parseTokens = new[]
    {
        " ", ".", ",", ";", ":", "!", "?", "{", "}",
        "[", "]", "'", "\"", "+", "-",
    }.ToFrozenSet();

    private static FrozenSet<string> _stopWords = new[]
    {
        "and", "any", "a", "as", "at", "an", "are",
        "the", "by", "but", "be",
        "for", "from",
        "have",
        "is", "in", "it", "its", "it's", "I",
        "make",
        "not",
        "of", "on", "or", "over",
        "to", "this", "that", "they",
        "with", "was", "you", "which", "will", "without",
        "you're",
    }
    .Concat(_parseTokens)
    .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

}
