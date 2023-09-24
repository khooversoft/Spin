namespace SpinCluster.sdk.Actors.Domain;

[GenerateSerializer, Immutable]
public sealed record DomainModel
{
    [Id(0)] public string Domain { get; init; } = null!;
    [Id(1)] public string? TenantId { get; init; } = null!;
}
