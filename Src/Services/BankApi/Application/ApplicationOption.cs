using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace BankApi.Application;

public record ApplicationOption
{
    public RunEnvironment RunEnvironment { get; init; }

    public string BankName { get; init; } = "Bank-First";

    public string ConfigStore { get; init; } = null!;

    public string DirectoryUrl { get; init; } = null!;

    public string DirectoryApiKey { get; init; } = null!;

    public string HostUrl { get; init; } = null!;

    public string ApiKey { get; init; } = null!;

    public string ArtifactUrl { get; init; } = null!;

    public string ArtifactApiKey { get; init; } = null!;
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

        option.BankName.VerifyNotEmpty($"{nameof(option.BankName)} is required");
        option.HostUrl.VerifyNotEmpty($"{nameof(option.HostUrl)} is required");
        option.ApiKey.VerifyNotEmpty($"{nameof(option.ApiKey)} is required");
        option.ArtifactUrl.VerifyNotNull(nameof(option.ArtifactUrl));
        option.ArtifactApiKey.VerifyNotNull(nameof(option.ArtifactApiKey));

        return option;
    }
}
