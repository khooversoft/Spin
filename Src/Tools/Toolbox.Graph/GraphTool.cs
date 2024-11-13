using System.Collections.Frozen;
using System.Collections.Immutable;
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

    public static IReadOnlyList<string> RemoveDeleteCommands(this IEnumerable<string> values) => values.NotNull()
        .Where(x => !TagsTool.HasRemoveFlag(x))
        .ToImmutableArray();

    public static IReadOnlyList<string> GetDeleteCommands(this IEnumerable<string> values) => values.NotNull()
        .Where(x => TagsTool.HasRemoveFlag(x))
        .Select(x => x[1..])
        .ToImmutableArray();

    public static IReadOnlyList<string> MergeCommands(this IEnumerable<string> newCommands, IEnumerable<string> currentCommands)
    {
        newCommands.NotNull();
        currentCommands.NotNull();

        var deleteCommands = newCommands.GetDeleteCommands();

        var list = newCommands
            .Concat(currentCommands)
            .Where(x => !TagsTool.HasRemoveFlag(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(x => !deleteCommands.Contains(x))
            .ToImmutableArray();

        return list;
    }

    public static string ToHashTag(string tag, string value) => $"{tag.NotEmpty()}-{RandomTag.Generate(6)}";
}
