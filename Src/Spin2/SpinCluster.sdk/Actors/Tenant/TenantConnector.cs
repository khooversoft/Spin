using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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

namespace SpinCluster.sdk.Actors.Tenant;

public class TenantConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<TenantConnector> _logger;

    public TenantConnector(IClusterClient client, ILogger<TenantConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Tenant}");

        group.MapDelete("/{nameId}", Delete);
        group.MapGet("/{nameId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string tenantId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<TenantId> option = TenantId.Create(tenantId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        ObjectId objectId = TenantModel.CreateId(option.Return());
        Option response = await _client.GetObjectGrain<ITenantActor>(objectId).Delete(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string tenantId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<TenantId> option = TenantId.Create(tenantId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        ObjectId objectId = TenantModel.CreateId(option.Return());
        Option<TenantModel> response = await _client.GetObjectGrain<ITenantActor>(objectId).Get(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(TenantModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = ObjectId.Create(model.TenantId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetObjectGrain<ITenantActor>(option.Return()).Set(model, context.TraceId);
        return response.ToResult();
    }
}
