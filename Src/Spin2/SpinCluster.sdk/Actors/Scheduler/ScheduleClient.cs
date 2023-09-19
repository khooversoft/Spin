using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public class ScheduleClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<ScheduleClient> _logger;

    public ScheduleClient(HttpClient client, ILogger<ScheduleClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> AddRunResult(RunResultModel model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/runResult")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> AddSchedule(ScheduleCreateModel model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/enqueue")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<ScheduleWorkModel>> AssignWork(string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(agentId)}/assign")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<ScheduleWorkModel>();

    public async Task<Option> Clear(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(principalId)}/clear")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> CompletedWork(AssignedCompleted completed, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/completed")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(completed)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<ScheduleWorkModel>> GetDetail(string workId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(workId)}/detail")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<ScheduleWorkModel>();

    public async Task<Option<SchedulesModel>> GetSchedules(ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/schedules")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<SchedulesModel>();
}
