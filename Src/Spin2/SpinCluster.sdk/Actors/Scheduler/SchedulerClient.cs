using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

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

    public async Task<Option<WorkAssignedModel>> AssignWork(string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(agentId)}/assign")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<WorkAssignedModel>();

    public async Task<Option> Clear(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(principalId)}/clear")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<SchedulesModel>> GetSchedules(ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/schedules")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<SchedulesModel>();
}
