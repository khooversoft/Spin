using Toolbox.Application;
using Toolbox.Tools;

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
        option.NotNull(nameof(option));

        option.ApiKey.NotEmpty($"{nameof(option.ApiKey)} is required");
        option.ConfigStore.NotEmpty($"{nameof(option.ConfigStore)} is required");

        option.Storage.NotNull($"{nameof(option.ConfigStore)} is required");
        option.Storage.AccountName.NotEmpty($"Storage.{nameof(option.Storage.AccountName)} is required");
        option.Storage.ContainerName.NotEmpty($"Storage.{nameof(option.Storage.ContainerName)} is required");
        option.Storage.AccountKey.NotEmpty($"Storage.{nameof(option.Storage.AccountKey)} is required");
        option.Storage.BasePath.NotEmpty($"Storage.{nameof(option.Storage.BasePath)} is required");

        option.IdentityStorage.NotNull($"{nameof(option.IdentityStorage)} is required");
        option.IdentityStorage.AccountName.NotEmpty($"IdentityStorage.{nameof(option.IdentityStorage.AccountName)} is required");
        option.IdentityStorage.ContainerName.NotEmpty($"IdentityStorage.{nameof(option.IdentityStorage.ContainerName)} is required");
        option.IdentityStorage.AccountKey.NotEmpty($"IdentityStorage.{nameof(option.IdentityStorage.AccountKey)} is required");
        option.IdentityStorage.BasePath.NotEmpty($"IdentityStorage.{nameof(option.IdentityStorage.BasePath)} is required");

        return option;
    }
}
