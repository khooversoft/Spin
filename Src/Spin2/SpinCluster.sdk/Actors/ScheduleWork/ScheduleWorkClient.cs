using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ScheduleWork;

public class ScheduleWorkClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<ScheduleWorkClient> _logger;

    public ScheduleWorkClient(HttpClient client, ILogger<ScheduleWorkClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> AddRunResult(RunResultModel model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.ScheduleWork}/runResult")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> CompletedWork(AssignedCompleted model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.ScheduleWork}/completed")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Create(ScheduleCreateModel model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.ScheduleWork}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Delete(string workId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.ScheduleWork}/{Uri.EscapeDataString(workId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<ScheduleWorkModel>> Get(string workId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.ScheduleWork}/{Uri.EscapeDataString(workId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<ScheduleWorkModel>();

    public async Task<Option> ReleaseAssign(string workId, bool force, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.ScheduleWork}/{Uri.EscapeDataString(workId)}/release/{force}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .PostAsync(context.With(_logger))
        .ToOption();
}
