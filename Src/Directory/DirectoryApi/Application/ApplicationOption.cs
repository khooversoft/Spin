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
        option.VerifyNotNull(nameof(option));

        option.ApiKey.VerifyNotEmpty($"{nameof(option.ApiKey)} is required");
        option.ConfigStore.VerifyNotEmpty($"{nameof(option.ConfigStore)} is required");

        option.Storage.VerifyNotNull($"{nameof(option.ConfigStore)} is required");
        option.Storage.AccountName.VerifyNotEmpty($"Storage.{nameof(option.Storage.AccountName)} is required");
        option.Storage.ContainerName.VerifyNotEmpty($"Storage.{nameof(option.Storage.ContainerName)} is required");
        option.Storage.AccountKey.VerifyNotEmpty($"Storage.{nameof(option.Storage.AccountKey)} is required");
        option.Storage.BasePath.VerifyNotEmpty($"Storage.{nameof(option.Storage.BasePath)} is required");

        option.IdentityStorage.VerifyNotNull($"{nameof(option.IdentityStorage)} is required");
        option.IdentityStorage.AccountName.VerifyNotEmpty($"IdentityStorage.{nameof(option.IdentityStorage.AccountName)} is required");
        option.IdentityStorage.ContainerName.VerifyNotEmpty($"IdentityStorage.{nameof(option.IdentityStorage.ContainerName)} is required");
        option.IdentityStorage.AccountKey.VerifyNotEmpty($"IdentityStorage.{nameof(option.IdentityStorage.AccountKey)} is required");
        option.IdentityStorage.BasePath.VerifyNotEmpty($"IdentityStorage.{nameof(option.IdentityStorage.BasePath)} is required");

        return option;
    }
}
