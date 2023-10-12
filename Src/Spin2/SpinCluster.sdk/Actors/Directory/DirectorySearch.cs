namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectorySearch
{
    [Id(0)] public string? NodeKey { get; init; }
    [Id(1)] public string? NodeTags { get; init; }
    [Id(2)] public string? FromKey { get; init; }
    [Id(3)] public string? EdgeTags { get; init; }
    [Id(4)] public string? ToKey { get; init; }
}
