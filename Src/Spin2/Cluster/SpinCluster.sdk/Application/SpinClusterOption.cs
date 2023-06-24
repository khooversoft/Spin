using SpinCluster.sdk.Services;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public record SpinClusterOption
{
    public string ApplicationInsightsConnectionString { get; init; } = null!;
    public string BootConnectionString { get; init; } = null!;
    public ClientSecretOption ClientCredentials { get; init; } = null!;
}

public static class SpinClusterOptionValidator
{
    public static Validator<SpinClusterOption> Validator { get; } = new Validator<SpinClusterOption>()
        .RuleFor(x => x.ApplicationInsightsConnectionString).NotEmpty()
        .RuleFor(x => x.BootConnectionString).Must(x => DatalakeLocation.ParseConnectionString(x).IsOk(), x => $"Connection string {x} is not valid")
        .RuleFor(x => x.ClientCredentials).Validate(ClientSecretOptionValidator.Validator)
        .Build();

    public static SpinClusterOption Verify(this SpinClusterOption option) => option.Action(x => Validator.Validate(option).ThrowOnError());
}



