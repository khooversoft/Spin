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
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;

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
        var context = new ScopeContext(traceId, _logger);
        Option<NameId> option = nameId.ToNameIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        ObjectId objectId = SubscriptionModel.CreateId(option.Return());
        Option response = await _client.GetObjectGrain<ISubscriptionActor>(objectId).Delete(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<NameId> option = nameId.ToNameIdIfValid(context.Location());
        if (option.IsError()) option.ToResult();

        ObjectId objectId = SubscriptionModel.CreateId(option.Return());
        Option<SubscriptionModel> response = await _client.GetObjectGrain<ISubscriptionActor>(objectId).Get(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(SubscriptionModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var response = await _client.GetObjectGrain<ISubscriptionActor>(model.SubscriptionId).Set(model, context.TraceId);
        return response.ToResult();
    }
}
