using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Subscription;

public class SubscriptionConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<SubscriptionConnector> _logger;

    public SubscriptionConnector(IClusterClient client, ILogger<SubscriptionConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Subscription}");

        group.MapDelete("/{nameId}", Delete);
        group.MapGet("/{agentId}/exist", Exist);
        group.MapGet("/{nameId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if (!IdPatterns.IsName(nameId)) return Results.BadRequest();

        string id = $"{SpinConstants.Schema.Subscription}:{nameId}";
        Option response = await _client.GetResourceGrain<ISubscriptionActor>(id).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Exist(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if (!ResourceId.IsValid(nameId, ResourceType.System, "agent")) return Results.BadRequest();

        string id = $"{SpinConstants.Schema.Subscription}:{nameId}";
        Option response = await _client.GetResourceGrain<ISubscriptionActor>(id).Exist(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if (!IdPatterns.IsName(nameId)) return Results.BadRequest();

        string id = $"{SpinConstants.Schema.Subscription}:{nameId}";
        Option<SubscriptionModel> response = await _client.GetResourceGrain<ISubscriptionActor>(id).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(SubscriptionModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<ISubscriptionActor>(model.SubscriptionId).Set(model, traceId);
        return response.ToResult();
    }
}
