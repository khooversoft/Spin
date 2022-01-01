using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Document;

/// <summary>
/// Id is a path of {domain}/{resource}
/// </summary>
public class DocumentId
{
    public DocumentId(string id)
    {
        id.VerifyNotEmpty(id);

        Id = id.ToLower();
        VerifyId(Id);
    }

    public string Id { get; }

    public string Domain => Id.Split('/')[0];

    public string Service => Id.Split('/')[1];

    public string Path => Id.Split('/').Skip(2).Join("'/");

    public IReadOnlyList<string> PathItems => Id.Split('/').Skip(2).ToArray();


    //  ///////////////////////////////////////////////////////////////////////////////////////////

    public override string ToString() => Id;

    public override bool Equals(object? obj) => obj is DocumentId id && Id == id.Id;

    public override int GetHashCode() => HashCode.Combine(Id);


    // ////////////////////////////////////////////////////////////////////////////////////////////

    public static explicit operator DocumentId(string id) => new DocumentId(id);

    public static explicit operator string(DocumentId documentId) => documentId.ToString();

    public static bool operator ==(DocumentId? left, DocumentId? right) => EqualityComparer<DocumentId>.Default.Equals(left, right);

    public static bool operator !=(DocumentId? left, DocumentId? right) => !(left == right);

    public static void VerifyId(string id)
    {
        id.IsDocumentIdValid()
            .VerifyAssert(x => x.Valid, x => x.Message);
    }
}


public static class DocumentIdUtility
{
    private const string _extension = ".json";

    public static string ToUrlEncoding(this DocumentId directoryId) => directoryId.Id.Replace('/', ':');

    public static DocumentId FromUrlEncoding(string id) => new DocumentId(id.Replace(':', '/'));

    public static string ToFileName(this DocumentId directoryId) => directoryId.Id + _extension;

    public static string FromFileName(string filename) => filename.EndsWith(_extension) ? filename[0..^_extension.Length] : filename;

    public static void VerifyDocumentId(this string id) => id.IsDocumentIdValid()
        .VerifyAssert(x => x.Valid, x => x.Message);

    public static (bool Valid, string Message) IsDocumentIdValid(this string documentId)
    {
        if (documentId.IsEmpty()) return (false, "Id required");

        string[] parts = documentId.Split('/');
        if (parts.Length <= 2) return (false, "Missing domain and/or service (ex {domain}/{service}[/{path}]");
        if (parts.Any(x => x.IsEmpty())) return (false, "One or more paths parts is empty");

        foreach (var item in parts)
        {
            if (!char.IsLetterOrDigit(item[0])) return (false, $"{item} Must start with letter or number");
            if (item.Length < 2) continue;

            if (!char.IsLetterOrDigit(item[^1])) return (false, "Must end with letter or number");



            if (!item.All(x => char.IsLetterOrDigit(x) || x == '.' || x == '-' || x == '@')) return (false, $"{item} is not valid, Valid Id must be letter, number, '.', '@', or '-'");
        }

        return (true, String.Empty);
    }

}
