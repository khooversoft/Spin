﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;

public static class ConnectorBaseExtensions
{
    public static void MapDelete(this RouteGroupBuilder group, ILogger logger, Func<string, ScopeContext, Task<SpinResponse>> call)
    {
        group.MapDelete("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var context = new ScopeContext(traceId, logger);
            Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
            if (option.IsError()) option.ToResult();

            var response = await call(objectId, context);
            return response.ToResult();
        });
    }

    public static void MapExist(this RouteGroupBuilder group, ILogger logger, Func<string, ScopeContext, Task<SpinResponse>> call)
    {
        group.MapGet("/exist/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var context = new ScopeContext(traceId, logger);
            Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
            if (option.IsError()) option.ToResult();

            var response = await call(objectId, context);
            return response.ToResult();
        });
    }

    public static void MapGet<T>(this RouteGroupBuilder group, ILogger logger, Func<string, ScopeContext, Task<SpinResponse<T>>> call)
    {
        group.MapGet("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var context = new ScopeContext(traceId, logger);
            Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
            if (option.IsError()) option.ToResult();

            var response = await call(objectId, context);
            return response.ToResult();
        });
    }

    public static void MapSet<T>(this RouteGroupBuilder group, ILogger logger, Func<string, T, ScopeContext, Task<SpinResponse>> call)
    {
        group.MapPost("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId, T model) =>
        {
            var context = new ScopeContext(traceId, logger);
            Option<ObjectId> option = objectId.ToObjectIdIfValid(context.Location());
            if (option.IsError()) option.ToResult();

            var response = await call(objectId, model, context);
            return response.ToResult();
        });
    }
}