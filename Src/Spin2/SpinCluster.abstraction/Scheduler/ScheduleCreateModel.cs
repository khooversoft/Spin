using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record ScheduleCreateModel
{
    [Id(0)] public string WorkId { get; init; } = $"{SpinConstants.Schema.ScheduleWork}:WKID-{Guid.NewGuid()}";
    [Id(1)] public string SchedulerId { get; init; } = null!;
    [Id(2)] public string SmartcId { get; init; } = null!;
    [Id(3)] public string PrincipalId { get; init; } = null!;
    [Id(4)] public DateTime? ValidTo { get; init; }
    [Id(5)] public DateTime? ExecuteAfter { get; init; }
    [Id(6)] public string SourceId { get; init; } = null!;
    [Id(7)] public string Command { get; init; } = null!;
    [Id(8)] public DataObjectSet Payloads { get; init; } = new DataObjectSet();
    [Id(9)] public string? Tags { get; init; }

    public static IValidator<ScheduleCreateModel> Validator { get; } = new Validator<ScheduleCreateModel>()
        .RuleFor(x => x.WorkId).ValidResourceId(ResourceType.System, SpinConstants.Schema.ScheduleWork)
        .RuleFor(x => x.SchedulerId).ValidResourceId(ResourceType.System, SpinConstants.Schema.Scheduler)
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Smartc)
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.SourceId).ValidName()
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.Payloads).Validate(DataObjectSet.Validator)
        .Build();
}

public static class ScheduleCreateModelExtensions
{
    public static Option Validate(this ScheduleCreateModel subject) => ScheduleCreateModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ScheduleCreateModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ScheduleWorkModel ConvertTo(this ScheduleCreateModel subject) => new ScheduleWorkModel
    {
        WorkId = subject.WorkId,
        SchedulerId = subject.SchedulerId,
        SmartcId = subject.SmartcId,
        ValidTo = subject.ValidTo,
        ExecuteAfter = subject.ExecuteAfter,
        SourceId = subject.SourceId,
        Command = subject.Command,
        Payloads = new DataObjectSet(subject.Payloads),
        Tags = subject.Tags,
    };
}
