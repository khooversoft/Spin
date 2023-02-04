using Directory.sdk.Model;
using System.Collections.Generic;
using Toolbox.Abstractions.Application;
using Toolbox.Abstractions.Tools;
using Toolbox.Extensions;

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
        option.NotNull();

        option.ConfigStore.NotEmpty(name: $"{nameof(option.ConfigStore)} is required");
        option.DirectoryUrl.NotEmpty(name: $"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.NotEmpty(name: $"{nameof(option.DirectoryApiKey)} is required");

        return option;
    }

    public static ApplicationOption Verify(this ApplicationOption option)
    {
        option.VerifyPartial();

        option.HostUrl.NotEmpty(name: $"{nameof(option.HostUrl)} is required");
        option.ApiKey.NotEmpty(name: $"{nameof(option.ApiKey)} is required");

        option.Storage.NotNull(name: $"{nameof(option.ConfigStore)} is required");
        option.Storage.Assert(x => x.Count > 0, $"{nameof(option.ConfigStore)} is required");

        option.Storage.ForEach(x => x.Verify());

        return option;
    }
}
