using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

public interface ISchedulerActor : IGrainWithStringKey
{
    Task<Option<WorkAssignedModel>> AssignWork(string agentId, string traceId);
    Task<Option> ChangeScheduleState(string workId, ScheduleEdgeType state, string traceId);
    Task<Option> CreateSchedule(ScheduleCreateModel work, string traceId);
    Task<Option> Delete(string principalId, string traceId);
    Task<Option<SchedulesResponseModel>> GetSchedules(string traceId);
    Task<Option> RemoveSchedule(string workId, string traceId);
    Task<Option> IsWorkAvailable(string traceId);
}


public static class SchedulerActorExtensions
{
    public static ISchedulerActor GetScheduleActor(this IClusterClient subject, string scheduleId) => subject
        .NotNull()
        .GetResourceGrain<ISchedulerActor>(scheduleId.NotEmpty());
}
