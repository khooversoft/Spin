﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;
using Microsoft.AspNetCore.Builder;
using static Azure.Core.HttpHeader;
using System.IO;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class PrincipalKeyConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<SubscriptionConnector> _logger;

    public PrincipalKeyConnector(IClusterClient client, ILogger<SubscriptionConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.PrincipalKey}");

        group.MapDelete("/{principalId}/{path?}", Delete);
        group.MapGet("/{principalId}/{path?}", Get);
        group.MapPost("/create", Create);
        group.MapPost("/", Update);

        return group;
    }

    private async Task<IResult> Delete(string principalId, string? path, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        principalId = Uri.UnescapeDataString(principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return Results.BadRequest("Invalid principal");
        if (path != null)
        {
            path = Uri.UnescapeDataString(path);
            if (!IdPatterns.IsPath(path)) return Results.BadRequest("Invalid path");
        }

        ResourceId resourceId = IdTool.CreatePublicKey(principalId, path);
        Option response = await _client.GetResourceGrain<IPrincipalKeyActor>(resourceId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string principalId, string? path, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        principalId = Uri.UnescapeDataString(principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return Results.BadRequest();
        if (path != null)
        {
            path = Uri.UnescapeDataString(path);
            if (!IdPatterns.IsPath(path)) return Results.BadRequest("Invalid path");
        }

        ResourceId resourceId = IdTool.CreatePublicKey(principalId, path);
        Option<PrincipalKeyModel> response = await _client.GetPublicKeyActor(resourceId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Create(PrincipalKeyCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var v = model.Validate().LogResult(context.Location());
        if (v.IsError()) return Results.BadRequest(v.Error);

        ResourceId resourceId = ResourceId.Create(model.PrincipalKeyId).Return();
        var response = await _client.GetPublicKeyActor(resourceId).Create(model, context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Update(PrincipalKeyModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<ResourceId> option = ResourceId.Create(model.PrincipalKeyId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetPublicKeyActor(option.Return()).Update(model, context.TraceId);
        return response.ToResult();
    }
}
