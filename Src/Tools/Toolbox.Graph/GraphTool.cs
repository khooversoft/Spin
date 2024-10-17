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
            .Prepend(GraphConstants.NodesDataBasePath)
            .Append(filePath)
            .Join('/');

        return storePath.ToLower();
    }

    public static string SetNodeCommand(string nodeKey, string? tags = null, string? base64 = null, string? dataName = null)
    {
        nodeKey.NotEmpty();
        dataName = dataName.ToNullIfEmpty() ?? "entity";
        var set = tags.IsNotEmpty() || base64.IsNotEmpty() ? "set " : null;

        string?[] parts = [
            tags,
            base64 != null ? $"{dataName} {{ '{base64}' }}" : null,
        ];

        string cmd = $"set node key={nodeKey} {set}" + parts.Where(x => x.IsNotEmpty()).Join(", ") + ';';
        return cmd;
    }

    public static string SetNodeCommand<T>(string nodeKey, T subject, string? tags = null, string? dataName = null)
    {
        nodeKey.NotEmpty();
        subject.NotNull();

        string base64 = subject.ToJson().ToBase64();
        dataName = dataName.ToNullIfEmpty() ?? "entity";

        var set = tags.IsNotEmpty() || base64.IsNotEmpty() ? "set " : null;

        string?[] parts = [
            tags,
            base64 != null ? $"{dataName} {{ '{base64}' }}" : null,
        ];

        string cmd = $"set node key={nodeKey} {set}" + parts.Where(x => x.IsNotEmpty()).Join(", ") + ';';
        return cmd;
    }

    public static string DeleteNodeCommand(string indexKey) => $"delete node ifexist key={indexKey.NotEmpty()};";

    public static IReadOnlyList<string> SetEdgeCommands(string fromKey, string toKey, string edgeType, string? tags = null)
    {
        fromKey.NotEmpty();
        toKey.NotEmpty();
        edgeType.NotEmpty();
        var set = tags.IsNotEmpty() ? "set " + tags : null;

        string cmd = $"set edge from={fromKey}, to={toKey}, type={edgeType} {set};";
        return [cmd];
    }

    public static IReadOnlyList<string> DeleteEdgeCommands(string fromKey, string toKey, string edgeType) => [
        $"delete edge ifexist from={fromKey.NotEmpty()}, to={toKey.NotEmpty()}, type={edgeType.NotEmpty()};"
        ];

    public static IReadOnlyList<string> CreateIndexCommands(string nodeKey, string indexKey)
    {
        nodeKey.NotEmpty();
        indexKey.NotEmpty();

        return [
            $"set node key={indexKey} set {GraphConstants.UniqueIndexEdgeType};",
            $"set edge from={indexKey}, to={nodeKey}, type={GraphConstants.UniqueIndexEdgeType};",
        ];
    }

    public static IReadOnlyList<string> DeleteIndexCommands(string nodeKey, string indexKey, string edgeType) => [
        $"delete edge ifexist from={indexKey.NotEmpty()}, to={nodeKey.NotEmpty()}, type={edgeType.NotEmpty()};"
    ];

    public static string SelectNodeCommand(string nodeKey, string? dataName = null)
    {
        nodeKey.NotEmpty();

        var cmd = dataName switch
        {
            string v => $"select (key={nodeKey}) return {v};",
            null => $"select (key={nodeKey});",
        };

        return cmd;
    }
}
