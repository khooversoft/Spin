using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Subscription;
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

        group.MapDelete("/{nameId}", Delete);
        group.MapGet("/{nameId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if( !ResourceId.IsValid(nameId, ResourceType.System, "agent") ) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IAgentActor>(nameId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if( !ResourceId.IsValid(nameId, ResourceType.System, "agent") ) return Results.BadRequest();

        Option<AgentModel> response = await _client.GetResourceGrain<IAgentActor>(nameId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(AgentModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var v = model.Validate();
        if( v.IsError() ) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<IAgentActor>(model.AgentId).Set(model, traceId);
        return response.ToResult();
    }
}
