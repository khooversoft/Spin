using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Models;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Agent;

public class AgentClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<AgentClient> _logger;

    public AgentClient(HttpClient client, ILogger<AgentClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}/{Uri.EscapeDataString(agentId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Exist(string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}/{Uri.EscapeDataString(agentId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<AgentModel>> Get(string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}/{Uri.EscapeDataString(agentId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<AgentModel>();

    public async Task<Option<AgentAssignmentModel>> GetAssignment(string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}/{Uri.EscapeDataString(agentId)}/assignment")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<AgentAssignmentModel>();

    public async Task<Option> IsActive(string agentId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}/{Uri.EscapeDataString(agentId)}/isActive")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Set(AgentModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();
}
