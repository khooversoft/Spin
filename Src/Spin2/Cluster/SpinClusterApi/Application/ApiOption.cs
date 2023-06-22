using Toolbox.Tools;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;

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

    public static ApiOption Verify(this ApiOption subject) =>
        subject.Action(x => Validator.Validate(x).ThrowOnError());
}