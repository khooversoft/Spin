using System.Collections.Generic;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ArtifactStore.Application;

public record Option
{
    public RunEnvironment Environment { get; init; }

    public string ApiKey { get; init; } = null!;

    public string? HostUrl { get; init; }

    public string ConfigStore { get; init; } = null!;

    public string HostServiceId { get; init; } = null!;
}


public static class OptionExtensions
{
    public static Option Verify(this Option option)
    {
        option.VerifyNotNull(nameof(option));

        option.ApiKey.VerifyNotEmpty($"{nameof(option.ApiKey)} is required");
        option.HostServiceId.VerifyNotEmpty($"{nameof(option.HostServiceId)} is required");
        option.ConfigStore.VerifyNotEmpty($"{nameof(option.HostServiceId)} is required");

        return option;
    }
}
