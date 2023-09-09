using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public class ScheduleClient
{
    protected readonly HttpClient _client;
    public ScheduleClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> AssignWork(string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(agentId)}/assign")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();

    public async Task<Option> CompletedWork(string workId, RunResultModel runResult, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/{Uri.EscapeDataString(workId)}/completed")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(runResult)
        .PostAsync(context)
        .ToOption();

    public async Task<Option> EnqueueSchedule(ScheduleWorkModel work, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/enqueue")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(work)
        .PostAsync(context)
        .ToOption();

    public async Task<Option<SchedulesModel>> GetDetail(string workId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/detail")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<SchedulesModel>();

    public async Task<Option<SchedulesModel>> GetSchedules(ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Scheduler}/schedules")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<SchedulesModel>();
}
