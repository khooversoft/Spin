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
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace SpinCluster.sdk.Actors.User;

public class UserConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<UserConnector> _logger;

    public UserConnector(IClusterClient client, ILogger<UserConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.User}");

        //group.MapDelete("/{nameId}", Delete);
        //group.MapGet("/{nameId}", Get);
        //group.MapPost("/", Set);

        return group;
    }

    //private async Task<IResult> Delete(string userEmail, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);
    //    Option<NameId> option = Princi.CreateIfValid(userEmail).LogResult(context.Location());
    //    if (option.IsError()) option.ToResult();

    //    ObjectId objectId = TenantModel.CreateId(option.Return());
    //    Option response = await _client.GetObjectGrain<ITenantActor>(objectId).Delete(context.TraceId);
    //    return response.ToResult();
    //}

    //public async Task<IResult> Get(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);
    //    Option<NameId> option = nameId.ToNameIdIfValid(context.Location());
    //    if (option.IsError()) option.ToResult();

    //    ObjectId objectId = UserModel.CreateId(option.Return());
    //    Option<TenantModel> response = await _client.GetObjectGrain<ITenantActor>(objectId).Get(context.TraceId);
    //    return response.ToResult();
    //}

    //public async Task<IResult> Set(TenantModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);

    //    var response = await _client.GetObjectGrain<ITenantActor>(model.TenantId).Set(model, context.TraceId);
    //    return response.ToResult();
    //}
}
