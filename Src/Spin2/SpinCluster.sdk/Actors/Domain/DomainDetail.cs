namespace SpinCluster.sdk.Actors.Domain;

public readonly record struct DomainDetail
{
    public string Domain { get; init; }
    public string? TenantId { get; init; }
}
