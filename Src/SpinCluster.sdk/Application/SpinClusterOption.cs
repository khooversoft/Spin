using SpinCluster.sdk.Services;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public record SpinClusterOption
{
    public string BootConnectionString { get; init; } = null!;
    public ClientSecretOption Credentials { get; init; } = null!;

    public static Validator<SpinClusterOption> Validator { get; } = new Validator<SpinClusterOption>()
        .RuleFor(x => x.BootConnectionString).Must(x => DatalakeEndpoint.Create(x).Validate().IsOk(), x => $"Connection string {x} is not valid")
        .RuleFor(x => x.Credentials).Validate(ClientSecretOption.Validator)
        .Build();
}

public static class SpinClusterOptionValidator
{
    public static Option Validate(this SpinClusterOption subject) => SpinClusterOption.Validator.Validate(subject).ToOptionStatus();
    public static bool Validate(this SpinClusterOption subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
