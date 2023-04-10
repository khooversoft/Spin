using Directory.sdk.Model;
using System;
using System.Collections.Generic;
using Toolbox.Abstractions.Application;
using Toolbox.Abstractions.Tools;

namespace Directory.sdk.Configuration;

public record DirectoryConfigurationOption
{
    public RunEnvironment RunEnvironment { get; init; }

    public string DirectoryUrl { get; init; } = null!;
    public string DirectoryApiKey { get; init; } = null!;

    public IReadOnlyList<KeyValuePair<string, ServiceRecord>> Services { get; init; } = Array.Empty<KeyValuePair<string, ServiceRecord>>();
}


public static class DirectoryConfigurationOptionExtensions
{
    public static DirectoryConfigurationOption Verify(this DirectoryConfigurationOption option)
    {
        option.NotNull();

        option.DirectoryUrl.NotEmpty(name: $"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.NotEmpty(name: $"{nameof(option.DirectoryApiKey)} is required");

        return option;
    }
}