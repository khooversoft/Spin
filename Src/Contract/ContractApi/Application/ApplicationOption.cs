using Toolbox.Application;
using Toolbox.Tools;

namespace ContractApi.Application;

public record ApplicationOption
{
    public RunEnvironment RunEnvironment { get; init; }

    public string DirectoryUrl { get; init; } = null!;

    public string DirectoryApiKey { get; init; } = null!;

    public string HostUrl { get; init; } = null!;

    public string ApiKey { get; init; } = null!;

    public string ArtifactUrl { get; init; } = null!;

    public string ArtifactApiKey { get; init; } = null!;
}


public static class ApplicationOptionExtensions
{
    public static ApplicationOption VerifyBootstrap(this ApplicationOption option)
    {
        option.NotNull();

        option.DirectoryUrl.NotEmpty(name: $"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.NotEmpty(name: $"{nameof(option.DirectoryApiKey)} is required");

        return option;
    }

    public static ApplicationOption Verify(this ApplicationOption option)
    {
        option.VerifyBootstrap();

        option.HostUrl.NotEmpty(name: $"{nameof(option.HostUrl)} is required");
        option.ApiKey.NotEmpty(name: $"{nameof(option.ApiKey)} is required");
        option.ArtifactUrl.NotNull(name: $"{nameof(option.ArtifactUrl)} is required");
        option.ArtifactApiKey.NotNull(name: $"{nameof(option.ArtifactApiKey)} is required");

        return option;
    }
}
