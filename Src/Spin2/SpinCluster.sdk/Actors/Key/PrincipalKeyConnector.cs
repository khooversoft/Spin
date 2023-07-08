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
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

public class PrincipalKeyConnector
{
    private readonly IClusterClient _client;
    private readonly ILogger<PrincipalKeyConnector> _logger;

    public PrincipalKeyConnector(IClusterClient client, ILogger<PrincipalKeyConnector> logger)
    {
        _client = client;
        _logger = logger;
    }

    public RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.ApiPath.PrincipalKey}");

        group.MapDelete(_logger, async (objectId, context) => await _client.GetGrain<IPrincipalKeyActor>(objectId).Delete(context.TraceId));
        group.MapExist(_logger, async (objectId, context) => await _client.GetGrain<IPrincipalKeyActor>(objectId).Exist(context.TraceId));
        group.MapGet<PrincipalKeyModel>(_logger, async (objectId, context) => await _client.GetGrain<IPrincipalKeyActor>(objectId).Get(context.TraceId));
        group.MapSet<PrincipalKeyRequest>(_logger, async (objectId, model, context) => await _client.GetGrain<IPrincipalKeyActor>(objectId).Update(model, context.TraceId));

        group.MapPost("/create/{*objectId}", async (string objectId, PrincipalKeyRequest model, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var context = new ScopeContext(traceId, _logger);
            Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
            if (option.IsError()) option.ToResult();

            var response = await _client.GetGrain<IPrincipalKeyActor>(objectId).Create(model, context.TraceId);
            return response.ToResult();
        });

        return group;
    }
}
