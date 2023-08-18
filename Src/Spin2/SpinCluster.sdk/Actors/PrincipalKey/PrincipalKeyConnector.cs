using Microsoft.AspNetCore.Http;
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

        group.MapDelete("/{principalId}", Delete);
        group.MapGet("/{principalId}", Get);
        group.MapPost("/create", Create);
        group.MapPost("/", Update);

        return group;
    }

    private async Task<IResult> Delete(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        principalId = Uri.UnescapeDataString(principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return Results.BadRequest();

        ResourceId resourceId = IdTool.CreatePublicKey(principalId);
        Option response = await _client.GetResourceGrain<IPrincipalKeyActor>(resourceId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        principalId = Uri.UnescapeDataString(principalId);
        if (!IdPatterns.IsName(principalId)) return Results.BadRequest();

        ResourceId resourceId = IdTool.CreatePublicKey(principalId);
        Option<PrincipalKeyModel> response = await _client.GetResourceGrain<IPrincipalKeyActor>(resourceId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Create(PrincipalKeyCreateModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<ResourceId> option = ResourceId.Create(model.KeyId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetResourceGrain<IPrincipalKeyActor>(option.Return()).Create(model, context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Update(PrincipalKeyModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<ResourceId> option = ResourceId.Create(model.KeyId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetResourceGrain<IPrincipalKeyActor>(option.Return()).Update(model, context.TraceId);
        return response.ToResult();
    }
}
