using SpinCluster.sdk.Actors.ScheduleWork;

namespace SpinCluster.sdk.Actors.Scheduler;

[GenerateSerializer, Immutable]
public sealed record SchedulesResponseModel
{
    // .Validate(AssignedCompleted.Validator)
    [Id(0)] public Dictionary<string, ScheduleEntry> ActiveItems { get; init; } = new Dictionary<string, ScheduleEntry>(StringComparer.OrdinalIgnoreCase);
    [Id(2)] public Dictionary<string, ScheduleEntry> CompletedItems { get; init; } = new Dictionary<string, ScheduleEntry>(StringComparer.OrdinalIgnoreCase);
}

[GenerateSerializer, Immutable]
public sealed record ScheduleEntry
{
    [Id(0)] public string WorkId { get; init; } = null!;
    [Id(1)] public DateTime CreateDate { get; init; } = DateTime.UtcNow;
    [Id(2)] public string ScheduleType { get; init; } = null!;
    [Id(3)] public AssignedModel? Assigned { get; init; }
}


public static class ScheduleEntryExtensions
{
    public static ScheduleEntry ConvertTo(this ScheduleWorkModel subject, string edgeType) => new ScheduleEntry
    {
        WorkId = subject.WorkId,
        CreateDate = subject.CreateDate,
        ScheduleType = edgeType,
        Assigned = subject.Assigned,
    };
}