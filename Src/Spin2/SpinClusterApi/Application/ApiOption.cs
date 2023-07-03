using SpinCluster.sdk.Services;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinClusterApi.Application;

public record ApiOption
{
    public string AppInsightsConnectionString { get; init; } = null!;
    public bool UseSwagger { get; init; }
    public string IpAddress { get; init; } = null!;
}


public static class ApiOptionExtensions
{
    public static Validator<ApiOption> Validator { get; } = new Validator<ApiOption>()
        .RuleFor(x => x.AppInsightsConnectionString).NotEmpty()
        .RuleFor(x => x.IpAddress).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this ApiOption subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}