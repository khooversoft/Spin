using System.Collections.Frozen;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch;

public class DocumentTokenizer
{
    private readonly WordTokenList _wordWeightList;

    public DocumentTokenizer() => _wordWeightList = new WordTokenList();
    public DocumentTokenizer(WordTokenList wordWeightList) => _wordWeightList = wordWeightList.NotNull();

    public IReadOnlyList<WordToken> Parse(string subject, IEnumerable<string>? tags = null)
    {
        if (subject.IsEmpty()) return Array.Empty<WordToken>();

        var newSubject = Clean(subject);

        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseUnicode()
            .Add(_parseTokens)
            .AddBlock('<', '>')
            .SetFilter(x => IsValid(x))
            .Parse(newSubject);

        var wordTokens = tokens
            .Select(x => _wordWeightList.Dictionary.TryGetValue(x.Value, out var wordWeight) switch
            {
                false => new WordToken(x.Value, 0),
                true => wordWeight,
            })
            .Concat((tags ?? Array.Empty<string>()).Select(x => new WordToken(x, 3)))
            .Select(Clean)
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
            case TokenValue v when v.TokenType == TokenType.Unicode: return false;

            case IToken v:
                if (v.Value.IsEmpty()) return false;
                if (v.Value.Length == 1) return false;
                if (_stopWords.Contains(v.Value)) return false;
                return true;

            default: return false;
        }
    }

    private static WordToken Clean(WordToken wordToken)
    {
        if (wordToken.Word.Length > 3 && wordToken.Word[0] == '\'') wordToken = new WordToken(wordToken.Word[1..], wordToken.Weight);
        if (wordToken.Word.Length > 3 && wordToken.Word[^1] == '\'') wordToken = new WordToken(wordToken.Word[..^1], wordToken.Weight);

        return wordToken;
    }

    private static string Clean(string line)
    {
        return DataTool.Filter(line, _ => true, convert);

        char convert(char chr)
        {
            if (_validCharacters.Contains(chr)) return chr;
            if (!char.IsAsciiLetterOrDigit(chr)) return ' ';
            return chr;
        }
    }

    private readonly static FrozenSet<char> _validCharacters = new char[]
    {
        '{', '}', '[', ']', '<', '>', '/'
    }.ToFrozenSet();

    private readonly static FrozenSet<string> _parseTokens = _validCharacters.Select(x => char.ToString(x)).ToFrozenSet();

    private readonly static FrozenSet<string> _stopWords = new[]
    {
        "and", "any", "a", "as", "at", "an", "are",
        "do", "does", "doesn't",
        "the", "by", "but", "be",
        "for", "from",
        "have", "however",
        "is", "in", "it", "its", "it's", "I",
        "make",
        "not",
        "of", "on", "or", "over",
        "to", "this", "that", "they", "those", "thoroughly", "thorough", "there", "then", "them", "their", "than",
        "us", "something", "some", "so", "should",
        "very",
        "with", "was", "you", "which", "will", "without", "www", "why", "whose", "while", "where", "whenever", "when", "what", "were", "well", "we",
        "ways", "we", "want", "would",
        "you're", "yourself", "your",
    }
    .Concat(_parseTokens)
    .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
}
