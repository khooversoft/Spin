using ContractHost.sdk.Event;
using Toolbox.Abstractions.Application;
using Toolbox.Abstractions.Tools;

namespace ContractHost.sdk.Model;

public record ContractHostOption
{
    public string ConfigFile { get; init; } = null!;
    public RunEnvironment RunEnvironment { get; init; }
    public string DirectoryUrl { get; init; } = null!;
    public string DirectoryApiKey { get; init; } = null!;


    // Specific contract operation
    public string EventPath { get; init; } = null!;
    public string DocumentId { get; init; } = null!;
    public string PrincipleId { get; init; } = null!;
    public string? EventConfig { get; init; }


    // From Directory
    public string ContractUrl { get; init; } = null!;
    public string ContractApiKey { get; init; } = null!;
}


public static class ContractOptionExtensions
{
    public static ContractHostOption VerifyFull(this ContractHostOption option)
    {
        option.NotNull();

        option.DirectoryUrl.NotEmpty(name: $"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.NotEmpty(name: $"{nameof(option.DirectoryApiKey)} is required");
        option.EventPath.NotEmpty(name: $"{nameof(option.EventPath)} is required");
        option.DocumentId.NotEmpty(name: $"{nameof(option.DocumentId)} is required");
        option.PrincipleId.NotEmpty(name: $"{nameof(option.PrincipleId)} is required");

        return option;
    }

    public static ContractHostOption Verify(this ContractHostOption option)
    {
        option.VerifyFull();

        option.ContractUrl.NotNull();
        option.ContractApiKey.NotNull();

        return option;
    }
}
