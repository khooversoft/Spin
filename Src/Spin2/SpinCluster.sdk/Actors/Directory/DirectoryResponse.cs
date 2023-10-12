using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryResponse
{
    [Id(0)] public IReadOnlyList<DirectoryNode> Nodes { get; init; } = Array.Empty<DirectoryNode>();
    [Id(1)] public IReadOnlyList<DirectoryEdge> Edges { get; init; } = Array.Empty<DirectoryEdge>();
}


[GenerateSerializer, Immutable]
public sealed record DirectoryNode
{
    public DirectoryNode() { }
    public DirectoryNode(string key, string? tags) => (Key, Tags) = (key, tags);

    [Id(0)] public string Key { get; init; } = null!;
    [Id(1)] public string? Tags { get; init; }

    public static IValidator<DirectoryNode> Validator { get; } = new Validator<DirectoryNode>()
        .RuleFor(x => x.Key).NotEmpty()
        .Build();
}


[GenerateSerializer, Immutable]
public sealed record DirectoryEdge
{
    public DirectoryEdge() { }
    public DirectoryEdge(string fromKey, string toKey, string? tags) => (FromKey, ToKey, Tags) = (fromKey, toKey, tags);

    [Id(0)] public string FromKey { get; init; } = null!;
    [Id(1)] public string ToKey { get; init; } = null!;
    [Id(2)] public string? Tags { get; init; }

    public static IValidator<DirectoryEdge> Validator { get; } = new Validator<DirectoryEdge>()
        .RuleFor(x => x.FromKey).NotEmpty()
        .RuleFor(x => x.ToKey).NotEmpty()
        .Build();
}


public static class DirectoryResponseExtensions
{
    public static Option Validate(this DirectoryNode subject) => DirectoryNode.Validator.Validate(subject).ToOptionStatus();
    public static bool Validate(this DirectoryNode subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static Option Validate(this DirectoryEdge subject) => DirectoryEdge.Validator.Validate(subject).ToOptionStatus();
    public static bool Validate(this DirectoryEdge subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
