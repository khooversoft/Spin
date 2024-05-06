using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphTool
{
    private static FrozenSet<(string chr, string replace)> _replaceMap = new[]
    {
        ( "/", "___" ),
        ( ":", "__" ),
        ( "$", "_DLR_" ),
    }.ToFrozenSet();

    private static string ToEncoding(string value) => _replaceMap.Aggregate(value, (x, y) => x.Replace(y.chr, y.replace));
    private static string ToDecoding(string value) => _replaceMap.Aggregate(value, (x, y) => x.Replace(y.replace, y.chr));


    public static string CreateFileId(string nodeKey, string name, string extension = ".json")
    {
        nodeKey.NotEmpty();
        name.Assert(x => IdPatterns.IsName(x), x => $"{x} invalid name");

        string file = PathTool.SetExtension(name, ".json");
        string filePath = ToEncoding($"{nodeKey}/{file}");

        string storePath = nodeKey
            .Split(new char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => ToEncoding(x))
            .Prepend("nodes")
            .Append(filePath)
            .Join('/');

        return storePath.ToLower();
    }

    public static Option<(string nodeKey, string name, string file)> DecodeFileId(string fileId)
    {
        string[] parts = fileId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3) return StatusCode.Conflict;

        (string nodeKey, string name, string file) result = parts.Length switch
        {
            3 => (ToDecoding(parts[2]), parts[1], ToDecoding(parts[2])),
            _ => (ToDecoding(parts[2] + "/" + parts[3]), parts[1], ToDecoding(parts[3])),
        };

        return result;
    }
}
