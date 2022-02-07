using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

namespace Toolbox.Document;

public static class DocumentIdTools
{
    public static string ToUrlEncoding(this DocumentId directoryId) => Uri.EscapeDataString((string)directoryId);

    public static DocumentId FromUrlEncoding(string id) => (DocumentId)Uri.UnescapeDataString(id);

    public static string ToJsonFileName(this DocumentId directoryId) => PathTools.SetExtension(directoryId.Path, ".json");

    public static string ToZipFileName(this DocumentId directoryId) => PathTools.SetExtension(directoryId.Path, ".zip");

    public static string RemoveExtension(this string path) => PathTools.RemoveExtension(path, ".json", ".zip");

    public static void VerifyDocumentId(this string id) => id.IsDocumentIdValid()
        .VerifyAssert(x => x.Valid, x => x.Message);

    public static DocumentId WithContainer(this DocumentId documentId, string container)
    {
        documentId.VerifyNotNull(nameof(documentId));
        container.VerifyNotEmpty(nameof(container));

        return new DocumentId(container + ":" + documentId.Path);
    }

    public static (bool Valid, string Message) IsDocumentIdValid(this string documentId)
    {
        const string syntax = "[{container}:]{path}[/{path}...]";

        if (documentId.IsEmpty()) return (false, "Id required");

        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .Add(":", "/")
            .Parse(documentId);

        int containerSymbolCount = tokens.Count(x => x.Value == ":");
        if (containerSymbolCount > 1) return (false, $"Invalid container, cannot have more then one ':', {syntax}");
        if (containerSymbolCount == 1 && (tokens.Count <= 1 || tokens[1].Value != ":")) return (false, $"Invalid container, {syntax}");
        if (containerSymbolCount == 0 && tokens.Where(filterSyntax).Count() == 0) return (false, $"No path, {syntax}");
        if (tokens.Last().Value == ":") return (false, $"Cannot end with ':', {syntax}");
        if (tokens.Last().Value == "/") return (false, $"Cannot end with '/', {syntax}");

        string? error = tokens
            .Where(filterSyntax)
            .Select(x => testVector(x.Value))
            .FirstOrDefault(x => x != null);

        return error != null ? (false, error) : (true, String.Empty);

        static bool filterSyntax(IToken token) => token.Value != ":" && token.Value != "/";

        static string? testVector(string item)
        {
            if (!char.IsLetterOrDigit(item[0])) return $"{item} Must start with letter or number";
            if (item.Length < 2) return null;

            if (!char.IsLetterOrDigit(item[^1])) return "Must end with letter or number";

            if (!item.All(x => char.IsLetterOrDigit(x) || x == '.' || x == '-' || x == '@')) return $"{item} is not valid, Valid Id must be letter, number, '.', '@', or '-'";
            return null;
        }
    }
}
