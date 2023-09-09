using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

[GenerateSerializer, Immutable]
public sealed record SmartcRunResultModel
{
    [Id(0)] public string SmartcId { get; init; } = null!;
    [Id(1)] public DateTime CompletedDate { get; init; } = DateTime.UtcNow;
    [Id(2)] public StatusCode StatusCode { get; init; }
    [Id(3)] public string Message { get; init; } = null!;

    public static IValidator<SmartcRunResultModel> Validator { get; } = new Validator<SmartcRunResultModel>()
        .RuleFor(x => x.SmartcId).NotEmpty()
        .RuleFor(x => x.Message).NotEmpty()
        .Build();
}


public static class SmartcRunResultModellExtensions
{
    public static Option Validate(this SmartcRunResultModel subject) => SmartcRunResultModel.Validator.Validate(subject).ToOptionStatus();
}