using Directory.sdk.Model;
using System.Collections.Generic;
using Toolbox.Application;
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

    public IReadOnlyList<StorageRecord> Storage { get; init; } = null!;
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
        option.Storage.VerifyAssert(x => x.Count > 0, $"{nameof(option.ConfigStore)} is required");

        option.Storage
            .ForEach(x =>
            {
                x.AccountName.VerifyNotEmpty($"{nameof(x.AccountName)} is required");
                x.AccountName.VerifyNotEmpty($"{nameof(x.AccountName)} is required");
                x.ContainerName.VerifyNotEmpty($"{nameof(x.ContainerName)} is required");
                x.AccountKey.VerifyNotEmpty($"{nameof(x.AccountKey)} is required");
                x.BasePath.VerifyNotEmpty($"{nameof(x.BasePath)} is required");
            });

        return option;
    }
}
