using System.Diagnostics;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


[DebuggerDisplay("Key={Key}, Tags={Tags.ToString()}")]
public sealed record GraphNode : IGraphCommon
{
    public GraphNode() { }

    public GraphNode(string key, string? tags = null)
    {
        Key = key.NotNull();
        Tags = new Tags().Set(tags);
    }

    [JsonConstructor]
    public GraphNode(string key, Tags tags, DateTime createdDate, IReadOnlyList<string> fileIds)
    {
        Key = key.NotNull();
        Tags = tags.NotNull();
        CreatedDate = createdDate;
        FileIds = fileIds?.ToArray() ?? Array.Empty<string>();
    }

    public string Key { get; init; } = default!;
    public Tags Tags { get; init; } = new Tags();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<string> FileIds { get; init; } = Array.Empty<string>();

    public GraphNode Copy() => new GraphNode(Key, Tags.Clone(), CreatedDate, FileIds);
    public GraphNode WithMerged(GraphNode node) => this with
    {
        Tags = Tags.Clone().Set(node.Tags),
        FileIds = FileIds.Concat(node.FileIds).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
    };

    public GraphNode AddFileId(string fileId) => this with { FileIds = [.. this.FileIds, fileId.NotEmpty()] };
    public GraphNode RemoveFileId(string fileId) => this with { FileIds = this.FileIds.Where(x => fileId.EqualsIgnoreCase(x)).ToArray() };

    public bool Equals(GraphNode? obj)
    {
        bool result = obj is GraphNode subject &&
            Key == subject.Key &&
            Tags == subject.Tags &&
            CreatedDate == subject.CreatedDate &&
            Enumerable.SequenceEqual(FileIds.OrderBy(x => x), subject.FileIds.OrderBy(x => x));

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Key, Tags, CreatedDate);

    public static IValidator<GraphNode> Validator { get; } = new Validator<GraphNode>()
        .RuleFor(x => x.Key).NotNull()
        .RuleFor(x => x.Tags).NotNull()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}

public static class GraphNodeExtensions
{
    public static Option Validate(this GraphNode subject) => GraphNode.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphNode subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static bool FileIdExist(this GraphNode graphNode, string fileId) => graphNode.FileIds.Contains(fileId, StringComparer.OrdinalIgnoreCase);
}