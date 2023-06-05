using Toolbox.Azure.Identity;

namespace ObjectStore.sdk.Application;

public record ObjectStoreOption
{
    public ClientSecretOption ClientIdentity { get; init; } = null!;
    public IReadOnlyList<DomainProfile> DomainProfiles { get; init; } = Array.Empty<DomainProfile>();
}

public record DomainProfile
{
    public string DomainName { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
}
