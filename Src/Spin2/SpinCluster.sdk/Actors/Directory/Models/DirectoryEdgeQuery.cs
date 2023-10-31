namespace SpinCluster.sdk.Actors.Directory.Models;

[GenerateSerializer, Immutable]
public record DirectoryEdgeQuery
{
    [Id(0)] public string? FromKey { get; init; }
    [Id(1)] public string? ToKey { get; init; }
    [Id(2)] public string? MatchEdgeType { get; init; }
    [Id(3)] public string? MatchTags { get; init; }
}
