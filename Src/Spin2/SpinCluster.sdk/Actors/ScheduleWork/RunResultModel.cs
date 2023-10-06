using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ScheduleWork;

[GenerateSerializer, Immutable]
public sealed record RunResultModel
{
    [Id(0)] public string Id { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(3)] public string WorkId { get; init; } = null!;
    [Id(4)] public DateTime CompletedDate { get; init; } = DateTime.UtcNow;
    [Id(5)] public StatusCode StatusCode { get; init; }
    [Id(2)] public string? AgentId { get; init; }
    [Id(6)] public string? Message { get; init; }
    [Id(7)] public DataObjectSet Payloads { get; init; } = new DataObjectSet();

    public static IValidator<RunResultModel> Validator { get; } = new Validator<RunResultModel>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.WorkId).NotEmpty()
        .RuleFor(x => x.CompletedDate).ValidDateTime()
        .RuleFor(x => x.StatusCode).ValidEnum()
        .RuleFor(x => x.AgentId).ValidResourceIdOption(ResourceType.System, "agent")
        .RuleFor(x => x.Payloads).Validate(DataObjectSet.Validator)
        .Build();
}


public static class RunResultModelExtensions
{
    public static Option Validate(this RunResultModel work) => RunResultModel.Validator.Validate(work).ToOptionStatus();

    public static bool Validate(this RunResultModel work, out Option result)
    {
        result = work.Validate();
        return result.IsOk();
    }
}