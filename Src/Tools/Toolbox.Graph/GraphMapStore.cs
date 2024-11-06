using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


internal class GraphMapStore
{
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly IGraphHost _graphContext;
    private readonly string _mapDatabasePath;
    private readonly ILogger _logger;
    private string? _currentETag;

    public GraphMapStore(IGraphHost graphContext, ILogger logger)
    {
        _graphContext = graphContext.NotNull();
        _logger = logger.NotNull();

        _mapDatabasePath = GraphConstants.MapDatabasePath;
    }

    public async Task<Option> Get(ScopeContext context)
    {
        context = context.With(_logger);

        await _resetEvent.WaitAsync(context.CancellationToken);
        try
        {
            context.LogInformation("Reading graph data file={mapDatabasePath}", _mapDatabasePath);

            var readMapSerializer = await _graphContext.FileStore.Get(_mapDatabasePath, context);
            if (readMapSerializer.IsNotFound()) return await InternalSet(context);

            if (readMapSerializer.IsError()) return readMapSerializer.ToOptionStatus();

            _currentETag = readMapSerializer.Return().ETag;
            GraphSerialization graphSerialization = readMapSerializer.Return().ToObject<GraphSerialization>();
            GraphMap newMap = graphSerialization.FromSerialization();

            context.LogInformation("Read graph data file={mapDatabasePath}", _mapDatabasePath);
            _graphContext.SetMap(newMap);

            return StatusCode.OK;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<Option> Set(ScopeContext context)
    {
        context = context.With(_logger);

        await _resetEvent.WaitAsync(context.CancellationToken);
        try
        {
            var result = await InternalSet(context);
            return result;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    private async Task<Option> InternalSet(ScopeContext context)
    {
        context = context.With(_logger);

        context.LogInformation("Writing graph data file={mapDatabasePath}", _mapDatabasePath);

        GraphSerialization graphSerialization = _graphContext.Map.ToSerialization();
        var writeMapSerializer = await _graphContext.FileStore.Set(_mapDatabasePath, graphSerialization.ToDataETag(_currentETag), context);
        if (writeMapSerializer.IsError()) return writeMapSerializer.ToOptionStatus();

        string newETag = writeMapSerializer.Return();
        Interlocked.Exchange(ref _currentETag, newETag);

        context.LogInformation("Write graph data file={mapDatabasePath}, eTag={etag}", _mapDatabasePath, _currentETag);
        return StatusCode.OK;
    }
}
