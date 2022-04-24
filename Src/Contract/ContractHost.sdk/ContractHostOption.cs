using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Tools;

namespace ContractHost.sdk;

public record ContractHostOption
{
    public string ConfigFile { get; init; } = null!;
    
    public RunEnvironment RunEnvironment { get; init; }

    public string DirectoryUrl { get; init; } = null!;

    public string DirectoryApiKey { get; init; } = null!;

    public string EventName { get; init; } = null!;

    public string ContractUrl { get; init; } = null!;

    public string ContractApiKey { get; init; } = null!;
}


public static class ContractOptionExtensions
{
    public static ContractHostOption VerifyBootstrap(this ContractHostOption option)
    {
        option.VerifyNotNull(nameof(option));

        option.DirectoryUrl.VerifyNotEmpty($"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.VerifyNotEmpty($"{nameof(option.DirectoryApiKey)} is required");
        option.EventName.VerifyNotEmpty($"{nameof(option.DirectoryApiKey)} is required");

        return option;
    }

    public static ContractHostOption Verify(this ContractHostOption option)
    {
        option.VerifyBootstrap();

        option.ContractUrl.VerifyNotNull(nameof(option.ContractUrl));
        option.ContractApiKey.VerifyNotNull(nameof(option.ContractApiKey));

        return option;
    }
}
