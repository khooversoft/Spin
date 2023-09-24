using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Agent;

public class AgentConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<AgentConnector> _logger;

    public AgentConnector(IClusterClient client, ILogger<AgentConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Agent}");

        group.MapDelete("/{agentId}", Delete);
        group.MapGet("/{agentId}/exist", Exist);
        group.MapGet("/{agentId}", Get);
        group.MapGet("/{agentId}/isActive", IsActive);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string agentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        agentId = Uri.UnescapeDataString(agentId);
        if (!ResourceId.IsValid(agentId, ResourceType.System, "agent")) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IAgentActor>(agentId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Exist(string agentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        agentId = Uri.UnescapeDataString(agentId);
        if (!ResourceId.IsValid(agentId, ResourceType.System, "agent")) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IAgentActor>(agentId).Exist(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string agentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        agentId = Uri.UnescapeDataString(agentId);
        if (!ResourceId.IsValid(agentId, ResourceType.System, "agent")) return Results.BadRequest();

        Option<AgentModel> response = await _client.GetResourceGrain<IAgentActor>(agentId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> IsActive(string agentId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        agentId = Uri.UnescapeDataString(agentId);
        if (!ResourceId.IsValid(agentId, ResourceType.System, "agent")) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IAgentActor>(agentId).IsActive(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(AgentModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<IAgentActor>(model.AgentId).Set(model, traceId);
        return response.ToResult();
    }
}
