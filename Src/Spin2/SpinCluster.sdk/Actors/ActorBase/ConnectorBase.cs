using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Resource;
using SpinCluster.sdk.Types;
using Toolbox.Tools.Zip;
using Toolbox.Tools;
using Toolbox.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using SpinCluster.sdk.Application;
using Microsoft.AspNetCore.Http;
using Toolbox.Azure.DataLake;
using SpinCluster.sdk.Actors.Search;
using System.Reflection;

namespace SpinCluster.sdk.Actors.ActorBase;

public interface IActionOperation<T> : IGrainWithStringKey
{
    Task<SpinResponse> Delete(string traceId);
    Task<SpinResponse<T>> Get(string traceId);
    Task<SpinResponse> Set(T model, string traceId);
    Task<SpinResponse> Exist(string traceId);
}

public abstract class ConnectorBase<T, TActor> where TActor : IActionOperation<T>
{
    protected readonly IClusterClient _client;
    protected readonly ILogger _logger;
    protected readonly string _rootPath;

    public ConnectorBase(IClusterClient client, string rootPath, ILogger logger)
    {
        _client = client.NotNull();
        _rootPath = rootPath.NotEmpty();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{_rootPath}");

        group.MapDelete(_logger, async (objectId, context) => await _client.GetGrain<TActor>(objectId).Delete(context.TraceId));
        group.MapExist(_logger, async (objectId, context) => await _client.GetGrain<TActor>(objectId).Exist(context.TraceId));
        group.MapGet<T>(_logger, async (objectId, context) => await _client.GetGrain<TActor>(objectId).Get(context.TraceId));
        group.MapSet<T>(_logger, async (objectId, model, context) => await _client.GetGrain<TActor>(objectId).Set(model, context.TraceId));

        return group;
    }
}
