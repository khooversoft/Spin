using System.Text.RegularExpressions;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Protocol;

public static class DocumentIdTools
{
    public static string ToUrlEncoding(this DocumentId directoryId) => Uri.EscapeDataString((string)directoryId);

    public static DocumentId FromUrlEncoding(string id) => (DocumentId)Uri.UnescapeDataString(id);

    public static string ToJsonFileName(this DocumentId directoryId) => PathTools.SetExtension(directoryId.Path, ".json");

    public static string ToZipFileName(this DocumentId directoryId) => PathTools.SetExtension(directoryId.Path, ".zip");

    public static string RemoveExtension(this string path) => PathTools.RemoveExtension(path, ".json", ".zip");

    public static void VerifyDocumentId(this string id) => id.IsDocumentIdValid()
        .Assert(x => x.Valid, x => x.Message);

    public static DocumentId WithContainer(this DocumentId documentId, string container)
    {
        documentId.NotNull();
        container.NotEmpty();

        return new DocumentId(container + ":" + documentId.Path);
    }

    public static (bool Valid, string Message) IsDocumentIdValid(this string documentId)
    {
        const string syntax = "[{container}:]{path}[/{path}...]";

        if (documentId.IsEmpty()) return (false, "Id required");

        return Regex.Match(documentId, ToolboxConstants.DocumentIdRegexPattern, RegexOptions.IgnoreCase).Success switch
        {
            true => (true, string.Empty),
            false => (false, syntax),
        };
    }
}
