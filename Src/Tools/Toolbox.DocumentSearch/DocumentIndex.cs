using System.Collections.Frozen;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch;

public class DocumentIndex
{
    private readonly DocumentTokenizer _tokenizer;

    public DocumentIndex(FrozenInvertedIndex<string, string> invertedIndex, DocumentTokenizer tokenizer, FrozenDictionary<string, DocumentReference> documentIndex)
    {
        InvertedIndex = invertedIndex.NotNull();
        _tokenizer = tokenizer.NotNull();
        Index = documentIndex.NotNull();
    }

    public FrozenInvertedIndex<string, string> InvertedIndex { get; }
    public FrozenDictionary<string, DocumentReference> Index { get; }

    public IReadOnlyList<DocumentReference> Search(string query)
    {
        IReadOnlyList<WordToken> wordTokens = _tokenizer.Parse(query);
        if (wordTokens.Count == 0) return Array.Empty<DocumentReference>();

        IReadOnlyList<DocumentReference> docs = wordTokens
            .SelectMany(x => InvertedIndex.Search(x.Word), (o, i) => (word: o, documentId: i))
            .GroupBy(x => x.documentId, StringComparer.OrdinalIgnoreCase)
            .Select(x => (documentId: x.Key, maxWeight: x.Max(x => x.word.Weight)))
            .OrderByDescending(x => x.maxWeight)
            .Take(20)
            .Select(x => Index[x.documentId])
            .ToArray();

        return docs;
    }
}
