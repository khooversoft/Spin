﻿using ContractHost.sdk.Event;
using Toolbox.Application;
using Toolbox.Tools;

namespace ContractHost.sdk.Model;

public record ContractHostOption
{
    public string ConfigFile { get; init; } = null!;
    public RunEnvironment RunEnvironment { get; init; }
    public string DirectoryUrl { get; init; } = null!;
    public string DirectoryApiKey { get; init; } = null!;


    // Specific contract operation
    public EventName EventName { get; init; }
    public string DocumentId { get; init; } = null!;
    public string PrincipleId { get; init; } = null!;
    public string? EventConfig { get; init; }


    // From Directory
    public string ContractUrl { get; init; } = null!;
    public string ContractApiKey { get; init; } = null!;
}


public static class ContractOptionExtensions
{
    public static ContractHostOption VerifyBootstrap(this ContractHostOption option)
    {
        option.NotNull();

        option.DirectoryUrl.NotEmpty(name: $"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.NotEmpty(name: $"{nameof(option.DirectoryApiKey)} is required");
        option.EventName.Assert(x => Enum.IsDefined(typeof(EventName), option.EventName), "Unknown option");
        option.DocumentId.NotEmpty(name: $"{nameof(option.DocumentId)} is required");
        option.PrincipleId.NotEmpty(name: $"{nameof(option.PrincipleId)} is required");

        return option;
    }

    public static ContractHostOption Verify(this ContractHostOption option)
    {
        option.VerifyBootstrap();

        option.ContractUrl.NotNull();
        option.ContractApiKey.NotNull();

        return option;
    }
}
