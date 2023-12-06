using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Data;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class SchedulerClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<SchedulerClient> _logger;

    public SchedulerClient(HttpClient client, ILogger<SchedulerClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> CreateSchedule(ScheduleCreateModel model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<WorkAssignedModel>> AssignWork(string schedulerId, string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(schedulerId)}/{Uri.EscapeDataString(agentId)}/assign")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<WorkAssignedModel>();

    public async Task<Option> Delete(string schedulerId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(schedulerId)}/{Uri.EscapeDataString(principalId)}/delete")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<SchedulesResponseModel>> GetSchedules(string schedulerId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(schedulerId)}/schedules")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<SchedulesResponseModel>();

    public async Task<Option> IsWorkAvailable(string schedulerId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(schedulerId)}/isWorkAvailable")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .ToOption();
}


public static class SchedulerClientExtensions
{
    public static async Task<Option<ScheduleAssigned>> LookForWork(this SchedulerClient client, ScheduleOption option, ScopeContext context)
    {
        client.NotNull();
        option.NotNull();
        if (!option.Validate(out var v)) return v.ToOptionStatus<ScheduleAssigned>();

        while (!context.IsCancellationRequested)
        {
            context.Location().LogInformation("Looking for work, scehdulerId={schedulerId}, agentId={agentId}", option.SchedulerId, option.AgentId);

            var result = await client.AssignWork(option.SchedulerId, option.AgentId, context);
            if (result.IsOk())
            {
                context.Location().LogInformation("Found work, scehdulerId={schedulerId}, agentId={agentId}, model={model}", option.SchedulerId, option.AgentId, result.Return());
                WorkAssignedModel assignment = result.Return();

                return new ScheduleAssigned
                {
                    ScheduleOption = option,
                    WorkAssigned = assignment,
                };
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        return StatusCode.NotFound;
    }

    public static async Task<Option> ClearAllWorkSchedules(this SchedulerClient schedulerClient, string schedulerId, ScheduleWorkClient scheduleWorkClient, ScopeContext context)
    {
        schedulerClient.NotNull();
        schedulerId.NotEmpty();
        scheduleWorkClient.NotNull();

        context.Location().LogInformation("Clearing all schedules for schedulerId={schedulerId}", schedulerId);

        var scheduleOptions = await schedulerClient.GetSchedules(schedulerId, context);
        if (scheduleOptions.IsError()) return scheduleOptions.ToOptionStatus();

        SchedulesResponseModel schedule = scheduleOptions.Return();

        string[] workIds = [.. schedule.ActiveItems.Keys, .. schedule.CompletedItems.Keys];
        foreach (var workId in workIds)
        {
            var deleteResult = await scheduleWorkClient.Delete(workId, context);
            if (deleteResult.IsError())
            {
                context.Location().LogWarning("Failed to delete workId={workId}", workId);
            }
        }

        return StatusCode.OK;
    }
}