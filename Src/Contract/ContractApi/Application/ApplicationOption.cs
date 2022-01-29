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
        option.VerifyNotNull(nameof(option));

        option.DirectoryUrl.VerifyNotEmpty($"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.VerifyNotEmpty($"{nameof(option.DirectoryApiKey)} is required");

        return option;
    }

    public static ApplicationOption Verify(this ApplicationOption option)
    {
        option.VerifyBootstrap();

        option.HostUrl.VerifyNotEmpty($"{nameof(option.HostUrl)} is required");
        option.ApiKey.VerifyNotEmpty($"{nameof(option.ApiKey)} is required");
        option.ArtifactUrl.VerifyNotNull(nameof(option.ArtifactUrl));
        option.ArtifactApiKey.VerifyNotNull(nameof(option.ArtifactApiKey));

        return option;
    }
}
