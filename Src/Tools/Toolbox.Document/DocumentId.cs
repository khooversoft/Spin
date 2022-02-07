using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Document;

/// <summary>
/// Id is a path of {domain}/{resource}
/// </summary>
public class DocumentId
{
    private string? _container;
    private string? _path;

    public DocumentId(string id)
    {
        id.VerifyNotEmpty(id);

        Id = id.ToLower();
        VerifyId(Id);
    }

    public string Id { get; }

    [JsonIgnore]
    public string? Container => _container ??= Id.Split(':').Func(x => x.Length == 1 ? null : x[0]);

    [JsonIgnore]
    public string Path => _path ??= Id.Split(':').Func(x => x.Length == 1 ? x[0] : x[1]);


    //  ///////////////////////////////////////////////////////////////////////////////////////////

    public override string ToString() => Id;

    public override bool Equals(object? obj) => obj is DocumentId documentId && Id == documentId.Id;

    public override int GetHashCode() => HashCode.Combine(Id);


    // ////////////////////////////////////////////////////////////////////////////////////////////

    public static explicit operator DocumentId(string documentId) => new DocumentId(documentId);

    public static explicit operator string(DocumentId documentId) => documentId.ToString();

    public static bool operator ==(DocumentId? left, DocumentId? right) => EqualityComparer<DocumentId>.Default.Equals(left, right);

    public static bool operator !=(DocumentId? left, DocumentId? right) => !(left == right);

    public static void VerifyId(string documentId)
    {
        documentId.IsDocumentIdValid()
            .VerifyAssert(x => x.Valid, x => x.Message);
    }
}
