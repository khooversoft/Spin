﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Models;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public class SmartcConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<SmartcConnector> _logger;

    public SmartcConnector(IClusterClient client, ILogger<SmartcConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Smartc}");

        group.MapDelete("/{smartcId}", Delete);
        group.MapGet("/{smartcId}/exist", Exist);
        group.MapGet("/{smartcId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string smartcId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        smartcId = Uri.UnescapeDataString(smartcId);
        if (!ResourceId.IsValid(smartcId, ResourceType.DomainOwned, "smartc")) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISmartcActor>(smartcId).Delete(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Exist(string smartcId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        smartcId = Uri.UnescapeDataString(smartcId);
        if (!ResourceId.IsValid(smartcId, ResourceType.DomainOwned, "smartc")) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISmartcActor>(smartcId).Exist(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Get(string smartcId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        smartcId = Uri.UnescapeDataString(smartcId);
        if (!ResourceId.IsValid(smartcId, ResourceType.DomainOwned, "smartc")) return Results.BadRequest();

        Option<SmartcModel> response = await _client.GetResourceGrain<ISmartcActor>(smartcId).Get(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Set(SmartcModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var v = model.Validate();
        if (v.IsError()) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<ISmartcActor>(model.SmartcId).Set(model, traceId);
        return response.ToResult();
    }
}
