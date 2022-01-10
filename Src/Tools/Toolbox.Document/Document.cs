using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace Toolbox.Document;

public class Document
{
    public DocumentId DocumentId { get; init; } = null!;

    public IReadOnlyDictionary<string, string> Properties { get; init; } = null!;

    public string ObjectClass { get; init; } = null!;

    public byte[] Data { get; init; } = null!;

    public byte[] Hash { get; init; } = null!;

    public override bool Equals(object? obj)
    {
        return obj is Document document &&
            DocumentId == document.DocumentId &&
            Properties.IsEqual(document.Properties) &&
            ObjectClass == document.ObjectClass &&
            Enumerable.SequenceEqual(Data, document.Data) &&
            Enumerable.SequenceEqual(Hash, document.Hash);
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, Properties, ObjectClass, Data, Hash);

    public static bool operator ==(Document? left, Document? right) => EqualityComparer<Document>.Default.Equals(left, right);

    public static bool operator !=(Document? left, Document? right) => !(left == right);
}
