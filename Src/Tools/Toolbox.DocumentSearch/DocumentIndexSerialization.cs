using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch;

public record DocumentIndexSerialization
{
    public IReadOnlyList<DocumentReference> Items { get; init; } = Array.Empty<DocumentReference>();
}


public static class DocumentIndexSerializationExtensions
{
    public static string ToJson(this DocumentIndex subject) => subject.ToSerialization().ToJson();

    public static DocumentIndexSerialization ToSerialization(this DocumentIndex subject) => new DocumentIndexSerialization
    {
        Items = subject.Index.Values.ToArray(),
    };

    public static DocumentIndex FromSerialization(this DocumentIndexSerialization subject)
    {
        subject.NotNull();

        var index = new DocumentIndexBuilder()
            .Action(x => subject.Items.ForEach(y => x.Add(y)))
            .Build();

        return index;
    }
}
