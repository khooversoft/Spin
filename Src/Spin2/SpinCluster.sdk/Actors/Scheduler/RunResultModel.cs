using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

[GenerateSerializer, Immutable]
public sealed record RunResultModel
{
    [Id(0)] public string AgentId { get; init; } = null!;
    [Id(1)] public DateTime CompletedDate { get; init; } = DateTime.UtcNow;
    [Id(2)] public StatusCode StatusCode { get; init; }
    [Id(3)] public string Message { get; init; } = null!;

    public static IValidator<RunResultModel> Validator { get; } = new Validator<RunResultModel>()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .RuleFor(x => x.Message).NotEmpty()
        .Build();
}


public static class RunResultModelExtensions
{
    public static Option Validate(this RunResultModel work) => RunResultModel.Validator.Validate(work).ToOptionStatus();
}