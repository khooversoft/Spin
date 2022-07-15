using System.Text.Json.Serialization;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Abstractions;

/// <summary>
/// Id is a path of {domain}/{resource}
/// </summary>
public class DocumentId
{
    private string? _container;
    private string? _path;
    private IReadOnlyList<string>? _vectors;

    public DocumentId(string id)
    {
        id.NotEmpty(name: id);

        Id = id.ToLower();
        VerifyId(Id);
    }

    public string Id { get; }

    [JsonIgnore]
    public string? Container => _container ??= Id.Split(':').Func(x => x.Length == 1 ? null : x[0]);

    [JsonIgnore]
    public string Path => _path ??= Id.Split(':').Func(x => x.Length == 1 ? x[0] : x[1]);

    [JsonIgnore]
    public IReadOnlyList<string> Vectors => _vectors ??= Path.Split('/');


    //  ///////////////////////////////////////////////////////////////////////////////////////////

    public override string ToString() => Id;
    public override bool Equals(object? obj) => obj is DocumentId documentId && Id == documentId.Id;
    public override int GetHashCode() => HashCode.Combine(Id);


    // ////////////////////////////////////////////////////////////////////////////////////////////

    public static explicit operator DocumentId(string documentId) => new DocumentId(documentId);
    public static explicit operator string(DocumentId documentId) => documentId.ToString();
    public static bool operator ==(DocumentId? left, DocumentId? right) => EqualityComparer<DocumentId>.Default.Equals(left, right);
    public static bool operator !=(DocumentId? left, DocumentId? right) => !(left == right);
    public static void VerifyId(string documentId) => documentId.IsDocumentIdValid().Assert(x => x.Valid, x => x.Message);
}


public static class DocumentIdExtensions
{
    public static RunEnvironment GetRunEnvironment(this DocumentId documentId)
    {
        documentId.NotNull();
        return (RunEnvironment)Enum.Parse(typeof(RunEnvironment), documentId.Path.Split('/').First());
    }

    public static DocumentId ToDocumentId(this string subject) => (DocumentId)subject;
}
