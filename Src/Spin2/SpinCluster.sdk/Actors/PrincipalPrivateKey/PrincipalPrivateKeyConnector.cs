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

        group.MapDelete("/{principalId}", Delete);
        group.MapGet("/{principalId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<PrincipalId> option = PrincipalId.Create(principalId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        ObjectId objectId = IdTool.CreatePrivateKeyId(option.Return());
        Option response = await _client.GetObjectGrain<IPrincipalPrivateKeyActor>(objectId).Delete(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<PrincipalId> option = PrincipalId.Create(principalId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        ObjectId objectId = IdTool.CreatePrivateKeyId(option.Return());
        Option<PrincipalPrivateKeyModel> response = await _client.GetObjectGrain<IPrincipalPrivateKeyActor>(objectId).Get(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(PrincipalPrivateKeyModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = ObjectId.Create(model.KeyId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetObjectGrain<IPrincipalPrivateKeyActor>(option.Return()).Set(model, context.TraceId);
        return response.ToResult();
    }
}
