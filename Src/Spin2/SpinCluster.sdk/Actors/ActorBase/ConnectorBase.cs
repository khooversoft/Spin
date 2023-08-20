using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;

public interface IActionOperation<T> : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option<T>> Get(string traceId);
    Task<Option> Set(T model, string traceId);
    Task<Option> Exist(string traceId);
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
