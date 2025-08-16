using System.Collections;
using System.Collections.Frozen;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch;

public class WordTokenList : IEnumerable<WordToken>
{
    public WordTokenList() => Dictionary = FrozenDictionary<string, WordToken>.Empty;

    public WordTokenList(IEnumerable<WordToken> values) => Dictionary = values.NotNull()
        .GroupBy(x => x.Word, StringComparer.OrdinalIgnoreCase)
        .Select(x => new WordToken(x.Key, x.Max(y => y.Weight)))
        .ToFrozenDictionary(x => x.Word, x => x, StringComparer.OrdinalIgnoreCase);

    public FrozenDictionary<string, WordToken> Dictionary { get; }
    public int Count => Dictionary.Count;

    public IReadOnlyList<WordToken> Lookup(IEnumerable<string> words)
    {
        words.NotNull();

        var result = words
            .Select(x => Dictionary.TryGetValue(x, out var wordWeight) ? wordWeight : null)
            .OfType<WordToken>()
            .ToArray();

        return result;
    }

    public string ToJson() => Dictionary
        .Select(x => x.Value)
        .OrderBy(x => x.Word, StringComparer.OrdinalIgnoreCase)
        .ToArray()
        .ToJsonFormat();

    public IEnumerator<WordToken> GetEnumerator()
    {
        foreach (var item in Dictionary)
        {
            yield return item.Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
