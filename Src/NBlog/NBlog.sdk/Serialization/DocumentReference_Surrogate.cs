using Toolbox.DocumentSearch;

namespace NBlog.sdk.Serialization;

[GenerateSerializer]
public struct DocumentReference_Surrogate
{
    public string DbName;
    public string DocumentId;
    public HashSet<WordToken> Words;
    public HashSet<string> Tags;
}


[RegisterConverter]
public sealed class DocumentReference_SurrogateConverter : IConverter<DocumentReference, DocumentReference_Surrogate>
{
    public DocumentReference ConvertFromSurrogate(in DocumentReference_Surrogate surrogate)
    {
        return new DocumentReference(surrogate.DbName, surrogate.DocumentId, surrogate.Words, surrogate.Tags);
    }

    public DocumentReference_Surrogate ConvertToSurrogate(in DocumentReference value) => new DocumentReference_Surrogate
    {
        DbName = value.DbName,
        DocumentId = value.DocumentId,
        Words = value.Words,
        Tags = value.Tags,
    };
}
