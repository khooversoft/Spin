using System.Collections.Frozen;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch;

public class DocumentIndexBuilder
{
    public DocumentTokenizer? Tokenizer { get; set; }
    public DocumentIndexBuilder SetTokenizer(DocumentTokenizer tokenizer) => this.Action(x => x.Tokenizer = tokenizer);

    public List<DocumentReference> DocumentReferences { get; } = new();
    public DocumentIndexBuilder Add(DocumentReference document) => this.Action(x => x.DocumentReferences.Add(document.Verify()));
    public DocumentIndexBuilder Add(IEnumerable<DocumentReference> documents) => this.Action(x => documents.NotNull().ForEach(y => x.DocumentReferences.Add(y)));

    public List<(string DocumentId, string Text, string[]? Tags)> Documents { get; } = new();
    public DocumentIndexBuilder Add(string documentId, string text, string[]? tags = null) => this.Action(x => x.Documents.Add((documentId.NotEmpty(), text.NotEmpty(), tags)));

    public DocumentIndex Build()
    {
        Tokenizer ??= new DocumentTokenizer();

        var newDocuments = Documents
            .Select(x => (r: x, words: Tokenizer.Parse(x.Text, x.Tags)))
            .Select(x => new DocumentReference("db", x.r.DocumentId, x.words, x.r.Tags))
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
}