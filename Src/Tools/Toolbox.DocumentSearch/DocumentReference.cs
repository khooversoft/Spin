using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.DocumentSearch;

public record DocumentReference
{
    public DocumentReference(string documentId, IEnumerable<WordToken> words, IEnumerable<string>? tags = null)
    {
        DocumentId = documentId.NotEmpty();

        Words = new HashSet<WordToken>(words.NotNull(), WordTokenComparer.Instance);
        Tags = new HashSet<string>(tags ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
    }

    [JsonConstructor]
    public DocumentReference(string documentId, HashSet<WordToken> words, HashSet<string> tags)
    {
        DocumentId = documentId.NotEmpty();
        Words = new HashSet<WordToken>(words.NotNull(), WordTokenComparer.Instance);
        Tags = new HashSet<string>(tags.NotNull(), StringComparer.OrdinalIgnoreCase);
    }

    public string DocumentId { get; init; } = null!;
    public HashSet<WordToken> Words { get; } = new HashSet<WordToken>(WordTokenComparer.Instance);
    public HashSet<string> Tags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static IValidator<DocumentReference> Validator { get; } = new Validator<DocumentReference>()
        .RuleFor(x => x.DocumentId).NotEmpty()
        .RuleForEach(x => x.Words).Validate(WordToken.Validator)
        .RuleForEach(x => x.Tags).NotEmpty()
        .Build();
}


public static class DocumentReferenceExtensions
{
    public static Option Validate(this DocumentReference subject) => DocumentReference.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DocumentReference subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static DocumentReference Verify(this DocumentReference subject) => subject.Action(x => x.Validate().ThrowOnError());
}