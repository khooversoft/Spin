using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

[GenerateSerializer, Immutable]
public sealed record RunResultModel
{
    [Id(0)] public string Id { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(2)] public string AgentId { get; init; } = null!;
    [Id(3)] public string WorkId { get; init; } = null!;
    [Id(4)] public DateTime CompletedDate { get; init; } = DateTime.UtcNow;
    [Id(5)] public StatusCode StatusCode { get; init; }
    [Id(6)] public string Message { get; init; } = null!;

    public static IValidator<RunResultModel> Validator { get; } = new Validator<RunResultModel>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .RuleFor(x => x.WorkId).NotEmpty()
        .RuleFor(x => x.CompletedDate).ValidDateTime()
        .RuleFor(x => x.StatusCode).ValidEnum()
        .RuleFor(x => x.Message).NotEmpty()
        .Build();
}


public static class RunResultModelExtensions
{
    public static Option Validate(this RunResultModel work) => RunResultModel.Validator.Validate(work).ToOptionStatus();
}