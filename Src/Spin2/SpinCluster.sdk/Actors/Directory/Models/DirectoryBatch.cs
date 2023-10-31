using System.Text.Json.Serialization;

namespace SpinCluster.sdk.Actors.Directory;

[JsonDerivedType(typeof(DirectoryNode), typeDiscriminator: "DirectoryNode")]
[JsonDerivedType(typeof(DirectoryEdge), typeDiscriminator: "DirectoryEdge")]
[JsonDerivedType(typeof(RemoveNode), typeDiscriminator: "RemoveNode")]
[JsonDerivedType(typeof(RemoveEdge), typeDiscriminator: "RemoveEdge")]
public interface IDirectoryGraph
{
}

[GenerateSerializer, Immutable]
public sealed record DirectoryBatch
{
    [Id(0)] public IReadOnlyList<IDirectoryGraph> Items { get; init; } = Array.Empty<IDirectoryGraph>();
}


public sealed record RemoveNode : IDirectoryGraph
{
    public string NodeKey { get; init; } = null!;
}

public sealed record RemoveEdge : IDirectoryGraph
{
    public Guid EdgeKey { get; init; }
}