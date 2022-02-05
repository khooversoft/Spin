using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Document;

public static class DocumentIdTools
{
    public static string ToUrlEncoding(this DocumentId directoryId) => directoryId.Id.Replace('/', ':');

    public static DocumentId FromUrlEncoding(string id) => new DocumentId(id.Replace(':', '/'));

    public static string ToJsonFileName(this DocumentId directoryId) => PathTools.SetExtension(directoryId.Id, ".json");

    public static string ToZipFileName(this DocumentId directoryId) => PathTools.SetExtension(directoryId.Id, ".zip");

    public static string RemoveExtension(this string path) => PathTools.RemoveExtension(path, ".json", ".zip");    

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
