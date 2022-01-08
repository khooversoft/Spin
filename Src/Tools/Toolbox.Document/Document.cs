using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Document;

public class Document
{
    public DocumentId DocumentId { get; init; } = null!;

    public IDictionary<string, string> Properties { get; init; } = null!;

    public string ObjectClass { get; init; } = null!;

    public byte[] Data { get; init; } = null!;

    public byte[] Hash { get; init; } = null!;

    public override bool Equals(object? obj)
    {
        return obj is Document document &&
            DocumentId == document.DocumentId &&

            Properties.Count == document.Properties.Count &&
            Properties.OrderBy(x => x.Key)
                .Zip(document.Properties.OrderBy(x => x.Key), (o, i) => (o, i))
                .All(x => x.o.Key == x.i.Key && x.o.Value == x.i.Value) &&

            ObjectClass == document.ObjectClass &&
            Enumerable.SequenceEqual(Data, document.Data) &&
            Enumerable.SequenceEqual(Hash, document.Hash);
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, Properties, ObjectClass, Data, Hash);

    public static bool operator ==(Document? left, Document? right) => EqualityComparer<Document>.Default.Equals(left, right);

    public static bool operator !=(Document? left, Document? right) => !(left == right);
}
