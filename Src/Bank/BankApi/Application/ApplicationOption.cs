using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace BankApi.Application;

public record ApplicationOption
{
    public RunEnvironment RunEnvironment { get; init; }

    public string BankName { get; init; } = null!;

    public string BankContainer { get; init; } = null!;

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
        option.NotNull(nameof(option));

        option.BankName.NotEmpty($"{nameof(option.BankName)} is required");
        option.ConfigStore.NotEmpty($"{nameof(option.ConfigStore)} is required");
        option.DirectoryUrl.NotEmpty($"{nameof(option.DirectoryUrl)} is required");
        option.DirectoryApiKey.NotEmpty($"{nameof(option.DirectoryApiKey)} is required");

        return option;
    }

    public static ApplicationOption Verify(this ApplicationOption option)
    {
        option.VerifyPartial();

        option.BankContainer.NotEmpty($"{nameof(option.BankContainer)} is required");
        option.HostUrl.NotEmpty($"{nameof(option.HostUrl)} is required");
        option.ApiKey.NotEmpty($"{nameof(option.ApiKey)} is required");
        option.ArtifactUrl.NotNull(nameof(option.ArtifactUrl));
        option.ArtifactApiKey.NotNull(nameof(option.ArtifactApiKey));

        return option;
    }
}
