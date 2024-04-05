using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphTool
{
    private static FrozenSet<(string chr, string replace)> _replaceMap = new []
    {
        ( "/", "_0SL" ),
        ( ":", "_0CO" ),
        ( "$", "_0DL" ),
    }.ToFrozenSet();

    private static string ToEncoding(string value) => _replaceMap.Aggregate(value, (x, y) => x.Replace(y.chr, y.replace));
    private static string ToDecoding(string value) => _replaceMap.Aggregate(value, (x, y) => x.Replace(y.replace, y.chr));

    public static string CreateFileId(string nodeKey, string name, string extension = ".json")
    {
        nodeKey.NotEmpty();
        name.Assert(x => IdPatterns.IsName(x), x => $"{x} invalid name");

        string file = ToEncoding(nodeKey.NotEmpty());
        file = PathTool.SetExtension(file, ".json");

        string storePath = nodeKey
            .Split(new char[] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Func(x => x.Length > 1 ? x[0..(x.Length - 1)] : Array.Empty<string>())
            .Join('/');

        string result = storePath.IsEmpty() switch
        {
            true => $"nodes/{name}/{file}",
            false => $"nodes/{name}/{storePath}/{file}",
        };

        return result.ToLower();
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

public static class EntityFileIdTool
{
    public static Tags RemoveFromTags(this EntityFileId entityFileId, Tags tags) => tags.Action(x => x.Remove(entityFileId.ToTagEncode()));
    public static Tags AddToTags(this EntityFileId entityFileId, Tags tags) => tags.Set(entityFileId.ToTagEncode(), entityFileId.EntityId);

    public static GraphNode DeleteEntityFileId(this GraphNode node, string fileId) => node with
    {
        //Tags = EntityFileId.CreateFromTagEncode(fileId).RemoveFromTags(node.Tags),
    };

    public static GraphNode SetEntityFileId(this GraphNode node, string fileId) => node with
    {
        //Tags = EntityFileId.CreateFromTagEncode(fileId).AddToTags(node.Tags),
    };
}


public sealed record EntityFileId
{
    private EntityFileId(string entityId) => EntityId = entityId
        .NotEmpty()
        .Split('/', StringSplitOptions.RemoveEmptyEntries)
        .Join('/');

    public string EntityId { get; }
    public string ToTagEncode() => $"{GraphConstants.EntityFileIdPrefix}{EntityId}";

    public static EntityFileId Create(string nodeKey, string name)
    {
        string fileId = GraphTool.CreateFileId(nodeKey, name);
        return new EntityFileId(fileId);
    }

    public static EntityFileId CreateFromTagEncode(string tag)
    {
        var fileId = tag.StartsWith(GraphConstants.EntityFileIdPrefix) switch
        {
            true => tag[GraphConstants.EntityFileIdPrefix.Length..],
            false => tag,
        };

        return new EntityFileId(fileId);
    }
}
