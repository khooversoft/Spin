using System.Collections.Frozen;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch;

public class DocumentIndexBuilder
{
    public TokenizeDocument? Tokenizer { get; set; }
    public DocumentIndexBuilder SetTokenizer(TokenizeDocument tokenizer) => this.Action(x => x.Tokenizer = tokenizer);

    public List<DocumentReference> DocumentReferences { get; } = new();
    public DocumentIndexBuilder Add(DocumentReference document) => this.Action(x => x.DocumentReferences.Add(document.Verify()));

    public List<(string DocumentId, string Text, string[]? Tags)> Documents { get; } = new();
    public DocumentIndexBuilder Add(string documentId, string text, string[]? tags = null) => this.Action(x => x.Documents.Add((documentId.NotEmpty(), text.NotEmpty(), tags)));

    public DocumentIndex Build()
    {
        Tokenizer.NotNull("required");

        var newDocuments = Documents
            .Select(x => (r: x, words: Tokenizer.Parse(x.Text, x.Tags)))
            .Select(x => new DocumentReference(x.r.DocumentId, x.words, x.r.Tags))
            .Concat(DocumentReferences)
            .GroupBy(x => x.DocumentId)
            .Select(x => x.Assert(y => y.Count() == 1, "Duplicate document ids").First())
            .ToArray();

        var index = new InvertedIndex<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);

        newDocuments
            .SelectMany(x => x.Words, (o, i) => (doc: o, word: i))
            .ForEach(x => index.Set(x.word.Word, x.doc.DocumentId));

        var frozenInvertedIndex = index.ToFrozenInvertedIndex();

        var frozenDocumentIndex = newDocuments.ToDictionary(x => x.DocumentId, x => x).ToFrozenDictionary();

        return new DocumentIndex(frozenInvertedIndex, Tokenizer, frozenDocumentIndex);
    }

    public static DocumentIndex BuildFromJson(string json, TokenizeDocument tokenizer)
    {
        json.NotEmpty();
        tokenizer.NotNull();

        var package = json.NotEmpty().ToObject<IReadOnlyList<DocumentReference>>().NotNull();

        var index = new DocumentIndexBuilder()
            .SetTokenizer(tokenizer)
            .Action(x => package.ForEach(y => x.Add(y)))
            .Build();

        return index;
    }
}