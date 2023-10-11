using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

[GenerateSerializer, Immutable]
public sealed record SchedulesModel
{
    // .Validate(AssignedCompleted.Validator)
    [Id(0)] public Dictionary<string, ScheduleEntry> ActiveItems { get; init; } = new Dictionary<string, ScheduleEntry>(StringComparer.OrdinalIgnoreCase);
    [Id(0)] public Dictionary<string, ScheduleEntry> AssignedItems { get; init; } = new Dictionary<string, ScheduleEntry>(StringComparer.OrdinalIgnoreCase);
    [Id(1)] public Dictionary<string, AssignedCompleted> CompletedItems { get; init; } = new Dictionary<string, AssignedCompleted>(StringComparer.OrdinalIgnoreCase);

    public static IValidator<SchedulesModel> Validator { get; } = new Validator<SchedulesModel>()
        .RuleForEach(x => x.ActiveItems).Must(x => x.Key.IsNotEmpty(), _ => "Key is required")
        .RuleForEach(x => x.ActiveItems).Must(x => x.Value.Validate().IsOk(), x => x.Value.Validate().ToString())
        .RuleForEach(x => x.CompletedItems).Must(x => x.Key.IsNotEmpty(), _ => "Key is required")
        .RuleForEach(x => x.CompletedItems).Must(x => x.Value.Validate().IsOk(), x => x.Value.Validate().ToString())
        .Build();
}

[GenerateSerializer, Immutable]
public sealed record ScheduleEntry
{
    [Id(0)] public string WorkId { get; init; } = null!;
    [Id(1)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(2)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromMinutes(30);
    [Id(3)] public DateTime? AssignedDate { get; init; }

    public bool IsActive => !IsAssignable();
    public bool IsAssignable() => AssignedDate == null || DateTime.UtcNow > (AssignedDate + TimeToLive);

    public override string ToString() => $"WorkId={WorkId}, IsActive={IsActive}, CreatedDate ={CreatedDate}, TimeToLive={TimeToLive}, AssignedDate={AssignedDate}";

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

    public static Option Validate(this ScheduleEntry subject) => ScheduleEntry.Validator.Validate(subject).ToOptionStatus();

    public static void RemoveAllReferences(this SchedulesModel subject, string workId)
    {
        subject.ActiveItems.Remove(workId);
        subject.AssignedItems.Remove(workId);
        subject.CompletedItems.Remove(workId);
    }

    public static void AddToActive(this SchedulesModel subject, ScheduleEntry scheduleEntry)
    {
        scheduleEntry = scheduleEntry with { AssignedDate = null };

        subject.ActiveItems[scheduleEntry.WorkId] = scheduleEntry;
        subject.AssignedItems.Remove(scheduleEntry.WorkId);
        subject.CompletedItems.Remove(scheduleEntry.WorkId);
    }

    public static void MoveToAssigned(this SchedulesModel subject, string workId)
    {
        if (!subject.ActiveItems.Remove(workId, out var activeItem)) return;

        if (activeItem.AssignedDate == null) activeItem = activeItem with { AssignedDate = DateTime.UtcNow };

        subject.AssignedItems[workId] = activeItem;
        subject.AssignedItems.Remove(workId);
        subject.CompletedItems.Remove(workId);
    }

    public static void MoveToCompleted(this SchedulesModel subject, AssignedCompleted assignedCompleted)
    {
        subject.CompletedItems[assignedCompleted.WorkId] = assignedCompleted;
        subject.ActiveItems.Remove(assignedCompleted.WorkId);
        subject.AssignedItems.Remove(assignedCompleted.WorkId);
    }

    public static bool ResetToActive(this SchedulesModel subject, string workId)
    {
        subject.CompletedItems.Remove(workId);

        switch ((subject.ActiveItems.ContainsKey(workId), subject.AssignedItems.ContainsKey(workId)))
        {
            case (false, false):
                return false;

            case (true, false):
                subject.ActiveItems[workId] = subject.ActiveItems[workId] with { AssignedDate = null };
                return true;

            case (false, true):
                if (!subject.AssignedItems.Remove(workId, out var item)) return false;
                subject.ActiveItems[workId] = item;
                return true;

            case (true, true):
                subject.ActiveItems[workId] = subject.ActiveItems[workId] with { AssignedDate = null };
                subject.AssignedItems.Remove(workId);
                return true;
        }
    }
}
