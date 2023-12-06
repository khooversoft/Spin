using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record WorkAssignedModel
{
    [Id(0)] public string WorkId { get; init; } = null!;
    [Id(1)] public string SmartcId { get; init; } = null!;
    [Id(3)] public string Command { get; init; } = null!;
    [Id(4)] public DataObjectSet Payloads { get; init; } = new DataObjectSet();
    [Id(5)] public string? Tags { get; init; }

    public static IValidator<WorkAssignedModel> Validator { get; } = new Validator<WorkAssignedModel>()
        .RuleFor(x => x.WorkId).ValidResourceId(ResourceType.System, SpinConstants.Schema.ScheduleWork)
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.Payloads).NotNull().Validate(DataObjectSet.Validator)
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
        Command = subject.Command,
        Payloads = new DataObjectSet(subject.Payloads),
        Tags = subject.Tags,
    };
}