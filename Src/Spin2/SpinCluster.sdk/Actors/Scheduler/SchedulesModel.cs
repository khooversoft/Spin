using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

[GenerateSerializer, Immutable]
public sealed record SchedulesModel
{
    [Id(0)] public List<ScheduleEntry> Active { get; init; } = new List<ScheduleEntry>();
    [Id(1)] public List<AssignedCompleted> CompletedItems { get; init; } = new List<AssignedCompleted>();

    public static IValidator<SchedulesModel> Validator { get; } = new Validator<SchedulesModel>()
        .RuleForEach(x => x.Active).Validate(ScheduleEntry.Validator)
        .RuleForEach(x => x.CompletedItems).Validate(AssignedCompleted.Validator)
        .Build();
}

[GenerateSerializer, Immutable]
public sealed record ScheduleEntry
{
    [Id(0)] public string WorkId { get; init; } = null!;
    [Id(1)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(2)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromMinutes(30);
    [Id(3)] public DateTime? AssignedDate { get; init; }

    public bool IsAssignable() => AssignedDate == null || DateTime.UtcNow > (AssignedDate + TimeToLive);

    public static IValidator<ScheduleEntry> Validator { get; } = new Validator<ScheduleEntry>()
        .RuleFor(x => x.WorkId).ValidResourceId(ResourceType.System, SpinConstants.Schema.ScheduleWork)
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}


public static class ScheduleWorkExtensions
{
    public static Option Validate(this SchedulesModel subject) => SchedulesModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SchedulesModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
