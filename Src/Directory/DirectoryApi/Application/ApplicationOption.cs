using Toolbox.Abstractions.Application;
using Toolbox.Abstractions.Tools;

namespace DirectoryApi.Application;

public record ApplicationOption
{
    public RunEnvironment RunEnvironment { get; init; }

    public string ApiKey { get; init; } = null!;

    public string? HostUrl { get; init; }

    public string ConfigStore { get; init; } = null!;

    public StorageOption Storage { get; init; } = null!;

    public StorageOption IdentityStorage { get; init; } = null!;
}

public record StorageOption
{
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string AccountKey { get; init; } = null!;
    public string BasePath { get; init; } = null!;
}


public static class ApplicationOptionExtensions
{
    public static ApplicationOption Verify(this ApplicationOption option)
    {
        option.NotNull();

        option.ApiKey.NotEmpty(name: $"{nameof(option.ApiKey)} is required");
        option.ConfigStore.NotEmpty(name: $"{nameof(option.ConfigStore)} is required");

        option.Storage.NotNull(name: $"{nameof(option.ConfigStore)} is required");
        option.Storage.AccountName.NotEmpty(name: $"Storage.{nameof(option.Storage.AccountName)} is required");
        option.Storage.ContainerName.NotEmpty(name: $"Storage.{nameof(option.Storage.ContainerName)} is required");
        option.Storage.AccountKey.NotEmpty(name: $"Storage.{nameof(option.Storage.AccountKey)} is required");
        option.Storage.BasePath.NotEmpty(name: $"Storage.{nameof(option.Storage.BasePath)} is required");

        option.IdentityStorage.NotNull(name: $"{nameof(option.IdentityStorage)} is required");
        option.IdentityStorage.AccountName.NotEmpty(name: $"IdentityStorage.{nameof(option.IdentityStorage.AccountName)} is required");
        option.IdentityStorage.ContainerName.NotEmpty(name: $"IdentityStorage.{nameof(option.IdentityStorage.ContainerName)} is required");
        option.IdentityStorage.AccountKey.NotEmpty(name: $"IdentityStorage.{nameof(option.IdentityStorage.AccountKey)} is required");
        option.IdentityStorage.BasePath.NotEmpty(name: $"IdentityStorage.{nameof(option.IdentityStorage.BasePath)} is required");

        return option;
    }
}
