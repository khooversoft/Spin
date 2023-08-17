﻿using System;
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

        ObjectId objectId = IdTool.CreateUserId(option.Return());
        Option response = await _client.GetObjectGrain<IUserActor>(objectId).Delete(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string principalId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<PrincipalId> option = PrincipalId.Create(principalId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        ObjectId objectId = IdTool.CreateUserId(option.Return());
        Option<UserModel> response = await _client.GetObjectGrain<IUserActor>(objectId).Get(context.TraceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(UserModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<ObjectId> option = ObjectId.Create(model.UserId).LogResult(context.Location());
        if (option.IsError()) option.ToResult();

        var response = await _client.GetObjectGrain<IUserActor>(option.Return()).Update(model, context.TraceId);
        return response.ToResult();
    }
}
