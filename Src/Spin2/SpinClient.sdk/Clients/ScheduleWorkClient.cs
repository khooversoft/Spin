using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

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


public static class ScheduleWorkClientExtensions
{
    public static async Task<Option> CompletedWork(this ScheduleWorkClient client, string agentId, string workId, Option option, string message, ScopeContext context)
    {
        client.NotNull();
        agentId.NotEmpty();
        workId.NotEmpty();
        message.NotEmpty();

        var completeStatus = new AssignedCompleted
        {
            AgentId = agentId,
            WorkId = workId,
            StatusCode = option.StatusCode,
            Message = message + (option.Error != null ? ", " + option.Error : string.Empty),
        };

        var updateOption = await client.CompletedWork(completeStatus, context);
        if (updateOption.IsError())
        {
            context.Location().LogError("Could not update complete work status on schedule, model={model}", completeStatus);
        }

        return updateOption;
    }

    public static async Task AddRunResult(this ScheduleWorkClient client, string workId, Option option, string message, ScopeContext context)
    {
        var response = new RunResultModel
        {
            WorkId = workId,
            StatusCode = option.StatusCode,
            Message = message + (option.Error != null ? ", " + option.Error : string.Empty),
        };

        var writeRunResult = await client.AddRunResult(response, context);
        if (writeRunResult.IsError())
        {
            context.Location().LogError("Failed to write 'RunResult' to loan contract, workId={workId}", workId);
        }
    }
}