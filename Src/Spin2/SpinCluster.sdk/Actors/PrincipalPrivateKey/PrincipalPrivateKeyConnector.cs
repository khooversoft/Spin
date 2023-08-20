using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class PrincipalPrivateKeyConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<PrincipalPrivateKeyConnector> _logger;

    public PrincipalPrivateKeyConnector(IClusterClient client, ILogger<PrincipalPrivateKeyConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.PrincipalPrivateKey}");

        group.MapDelete("/{principalId}/{path?}", Delete);
        group.MapGet("/{principalId}/{path?}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string principalId, string? path, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!IdPatterns.IsPrincipalId(principalId)) return Results.BadRequest();
        if (path != null)
        {
            path = Uri.UnescapeDataString(path);
            if (!IdPatterns.IsPath(path)) return Results.BadRequest("Invalid path");
        }

        ResourceId resourceId = IdTool.CreatePrivateKey(principalId, path);
        Option response = await _client.GetPrivateKeyActor(resourceId).Delete(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string principalId, string? path, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!IdPatterns.IsPrincipalId(principalId)) return Results.BadRequest();
        if (path != null)
        {
            path = Uri.UnescapeDataString(path);
            if (!IdPatterns.IsPath(path)) return Results.BadRequest("Invalid path");
        }

        ResourceId resourceId = IdTool.CreatePrivateKey(principalId, path);
        Option<PrincipalPrivateKeyModel> response = await _client.GetPrivateKeyActor(resourceId).Get(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(PrincipalPrivateKeyModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var v = model.Validate().LogResult(context.Location());
        if (v.IsError()) return Results.BadRequest(v.Error);

        ResourceId resourceId = ResourceId.Create(model.PrincipalPrivateKeyId).Return();
        var response = await _client.GetPrivateKeyActor(resourceId).Set(model, context.TraceId);
        return response.ToResult();
    }
}
