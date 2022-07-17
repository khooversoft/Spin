using Toolbox.Extensions;

namespace Toolbox.Abstractions;

public class Document
{
    public DocumentId DocumentId { get; init; } = null!;

    public string ObjectClass { get; init; } = null!;

    public byte[] Data { get; init; } = null!;

    public byte[] Hash { get; init; } = null!;

    public string? PrincipleId { get; init; }

    public override bool Equals(object? obj)
    {
        return obj is Document document &&
            DocumentId == document.DocumentId &&
            ObjectClass == document.ObjectClass &&
            Data.SequenceEqual(document.Data) &&
            Hash.SequenceEqual(document.Hash);
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, ObjectClass, Data, Hash);
    public static bool operator ==(Document? left, Document? right) => EqualityComparer<Document>.Default.Equals(left, right);
    public static bool operator !=(Document? left, Document? right) => !(left == right);
}
