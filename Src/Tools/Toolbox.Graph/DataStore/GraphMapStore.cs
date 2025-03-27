using Microsoft.Extensions.Logging;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


internal class GraphMapStore
{
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly IGraphHost _graphHost;
    private readonly string _mapDatabasePath;
    private readonly ILogger _logger;
    private string? _currentETag;

    public GraphMapStore(IGraphHost graphContext, ILogger logger)
    {
        _graphHost = graphContext.NotNull();
        _logger = logger.NotNull();

        _mapDatabasePath = GraphConstants.MapDatabasePath;
    }

    public async Task<Option> Get(ScopeContext context)
    {
        context = context.With(_logger);

        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        using var metric = context.LogDuration("graphMapStore-get");

        try
        {
            context.LogTrace("Reading graph data file={mapDatabasePath}", _mapDatabasePath);

            var readMapSerializer = await _graphHost.FileStore.File(_mapDatabasePath).Get(context).ConfigureAwait(false);
            if (readMapSerializer.IsNotFound()) return await InternalSet(context).ConfigureAwait(false);

            if (readMapSerializer.IsError()) return readMapSerializer.ToOptionStatus();

            _currentETag = readMapSerializer.Return().ETag;
            GraphSerialization graphSerialization = readMapSerializer.Return().ToObject<GraphSerialization>();
            GraphMap newMap = graphSerialization.FromSerialization();

            context.LogTrace("Read graph data file={mapDatabasePath}", _mapDatabasePath);
            _graphHost.SetMap(newMap);

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
        _graphHost.ReadOnly.Assert(x => x == false, "Cannot set map when read-only");

        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        try
        {
            var result = await InternalSet(context).ConfigureAwait(false);
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
        using var metric = context.LogDuration("graphMapStore-set");

        context.LogTrace("Writing graph data file={mapDatabasePath}", _mapDatabasePath);

        GraphSerialization graphSerialization = _graphHost.Map.ToSerialization();
        var writeMapSerializer = await _graphHost.FileStore.File(_mapDatabasePath).Set(graphSerialization.ToDataETag(_currentETag), context).ConfigureAwait(false);
        if (writeMapSerializer.IsError()) return writeMapSerializer.ToOptionStatus();

        string newETag = writeMapSerializer.Return();
        Interlocked.Exchange(ref _currentETag, newETag);

        context.LogTrace("Write graph data file={mapDatabasePath}, eTag={etag}", _mapDatabasePath, _currentETag);
        return StatusCode.OK;
    }
}
