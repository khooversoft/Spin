using System.Collections.Generic;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Artifact.Application;

public record ApplicationOption
{
    public RunEnvironment RunEnvironment { get; init; }

    public string ConfigStore { get; init; } = null!;

    public string DirectoryUrl { get; init; } = null!;

    public string DirectoryApiKey { get; init; } = null!;

    public string HostUrl { get; init; } = null!;

    public string ApiKey { get; init; } = null!;

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
    public static ApplicationOption VerifyPartial(this ApplicationOption option)
    {
        option.VerifyNotNull(nameof(option));

        option.ConfigStore.VerifyNotEmpty($"{nameof(option.ConfigStore)} is required");
        option.DirectoryUrl.VerifyNotEmpty($"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.VerifyNotEmpty($"{nameof(option.DirectoryApiKey)} is required");

        return option;
    }

    public static ApplicationOption Verify(this ApplicationOption option)
    {
        option.VerifyPartial();

        option.HostUrl.VerifyNotEmpty($"{nameof(option.HostUrl)} is required");
        option.ApiKey.VerifyNotEmpty($"{nameof(option.ApiKey)} is required");

        option.Storage.VerifyNotNull($"{nameof(option.ConfigStore)} is required");
        option.Storage.AccountName.VerifyNotEmpty($"{nameof(option.Storage.AccountName)} is required");
        option.Storage.ContainerName.VerifyNotEmpty($"{nameof(option.Storage.ContainerName)} is required");
        option.Storage.AccountKey.VerifyNotEmpty($"{nameof(option.Storage.AccountKey)} is required");
        option.Storage.BasePath.VerifyNotEmpty($"{nameof(option.Storage.BasePath)} is required");

        return option;
    }
}
