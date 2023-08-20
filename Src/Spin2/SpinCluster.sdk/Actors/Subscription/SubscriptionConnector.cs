﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
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
        group.MapGet("/{nameId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if (!IdPatterns.IsName(nameId)) return Results.BadRequest();

        ResourceId resourceId = IdTool.CreateSubscriptionId(nameId);
        Option response = await _client.GetResourceGrain<ISubscriptionActor>(resourceId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if (!IdPatterns.IsName(nameId)) return Results.BadRequest();

        ResourceId resourceId = IdTool.CreateSubscriptionId(nameId);
        Option<SubscriptionModel> response = await _client.GetResourceGrain<ISubscriptionActor>(resourceId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(SubscriptionModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<ResourceId> option = ResourceId.Create(model.SubscriptionId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetResourceGrain<ISubscriptionActor>(option.Return()).Set(model, traceId);
        return response.ToResult();
    }
}
