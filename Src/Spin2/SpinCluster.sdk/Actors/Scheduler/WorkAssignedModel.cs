using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

[GenerateSerializer, Immutable]
public sealed record WorkAssignedModel
{
    [Id(0)] public string WorkId { get; init; } = null!;
    [Id(1)] public string SmartcId { get; init; } = null!;
    [Id(2)] public string CommandType { get; init; } = null!;
    [Id(3)] public string Command { get; init; } = null!;

    public static IValidator<WorkAssignedModel> Validator { get; } = new Validator<WorkAssignedModel>()
        .RuleFor(x => x.WorkId).ValidResourceId(ResourceType.System, SpinConstants.Schema.ScheduleWork)
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .RuleFor(x => x.CommandType).Must(x => x == "args" || x.StartsWith("json:"), x => $"{x} is not valid, must be 'args' or 'json:{{type}}'")
        .RuleFor(x => x.Command).NotEmpty()
        .Build();
}


public static class WorkAssignedModelExternal
{
    public static Option Validate(this WorkAssignedModel subject) => WorkAssignedModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this WorkAssignedModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static WorkAssignedModel ConvertTo(this ScheduleWorkModel subject) => new WorkAssignedModel
    {
        WorkId = subject.WorkId,
        SmartcId = subject.SmartcId,
        CommandType = subject.CommandType,
        Command = subject.Command,
    };
}