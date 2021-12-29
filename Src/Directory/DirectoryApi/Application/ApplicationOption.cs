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
        option.Storage.AccountName.VerifyNotEmpty($"{nameof(option.Storage.AccountName)} is required");
        option.Storage.ContainerName.VerifyNotEmpty($"{nameof(option.Storage.ContainerName)} is required");
        option.Storage.AccountKey.VerifyNotEmpty($"{nameof(option.Storage.AccountKey)} is required");
        option.Storage.BasePath.VerifyNotEmpty($"{nameof(option.Storage.BasePath)} is required");

        return option;
    }
}
