using System.Text.Json.Nodes;
using Toolbox.Tools;

namespace Toolbox.DocumentContainer;

public sealed record Document
{
    public required string DocumentId { get; init; } = null!;
    public required string TypeName { get; init; } = null!;
    public required string Content { get; init; } = null!;
    public string? ETag { get; init; }
    public string? Tags { get; init; } = null!;

    public bool Equals(Document? obj)
    {
        return obj is Document document &&
               DocumentId == document.DocumentId &&
               TypeName == document.TypeName &&
               Content == document.Content &&
               ETag == document.ETag &&
               Tags == document.Tags;
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, TypeName, Content, ETag, Tags);
}
