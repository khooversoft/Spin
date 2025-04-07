using Microsoft.Extensions.Logging;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


public class GraphMapStore : IAsyncDisposable
{
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly ILogger<GraphMapStore> _logger;
    private readonly GraphHostOption _graphHostOption;
    private string? _currentETag;
    private GraphMap _map = new GraphMap();
    private GraphLeaseControl _leaseControl;

    public GraphMapStore(GraphLeaseControl leaseControl, GraphHostOption graphHostOption, ILogger<GraphMapStore> logger)
    {
        _leaseControl = leaseControl.NotNull();
        _graphHostOption = graphHostOption.NotNull();
        _logger = logger.NotNull();
    }

    public void SetMap(GraphMap map) => _map = map.NotNull();
    public GraphMap GetMapReference() => _map;

    public async Task<Option> AcquireExclusive(ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        using var metric = context.LogDuration("graphMapStore-AcquireExclusive");

        try
        {
            var leaseOption = await _leaseControl.AcquireExclusive(context).ConfigureAwait(false);
            if (leaseOption.IsError()) return leaseOption;

            var getResult = await LoadDatabase(context);
            if (getResult.IsError())
            {
                await _leaseControl.ReleaseExclusive(context).ConfigureAwait(false);
                return getResult;
            }

            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load exclusive lock");
            return StatusCode.InternalServerError;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<Option<IAsyncDisposable>> AcquireScope(ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        using var metric = context.LogDuration("graphMapStore-AcquireScope");

        try
        {
            var leaseOption = await _leaseControl.AcquireScope(context).ConfigureAwait(false);
            if (leaseOption.IsError()) return leaseOption;

            var getResult = await LoadDatabase(context);
            if (getResult.IsError()) return getResult.ToOptionStatus<IAsyncDisposable>();

            return leaseOption;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<Option> CheckpointMap(ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        using var metric = context.LogDuration("graphMapStore-CheckpointMap");

        try
        {
            var setResult = await SaveDatabase(context).ConfigureAwait(false);
            if (setResult.IsError())
            {
                setResult.LogStatus(context, "Failed to checkpoint map database");
                return setResult.ToOptionStatus();
            }

            return StatusCode.OK;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<Option> InitializeDatabase(ScopeContext context)
    {
        context = context.With(_logger);

        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        using var metric = context.LogDuration("graphMapStore-get");

        try
        {
            var loadOption = await LoadDatabase(context).ConfigureAwait(false);
            if (loadOption.IsOk()) return loadOption;

            context.LogTrace("Graph data file={mapDatabasePath} not found", GraphConstants.MapDatabasePath);
            if (_graphHostOption.ReadOnly) return (StatusCode.NotFound, $"Graph data file={GraphConstants.MapDatabasePath} not found");

            var saveOption = await SaveDatabase(context).ConfigureAwait(false);
            if (saveOption.IsError())
            {
                context.LogError("Failed to set map database, result={result}", saveOption);
                return saveOption.ToOptionStatus();
            }

            context.LogTrace("Created database file");
            DataETag dataETag = saveOption.Return();
            _currentETag = dataETag.ETag;

            GraphSerialization graphSerialization = dataETag.ToObject<GraphSerialization>();
            GraphMap newMap = graphSerialization.FromSerialization();
            Interlocked.Exchange(ref _map, newMap);

            context.LogTrace("Read graph data file={mapDatabasePath}", GraphConstants.MapDatabasePath);
            return StatusCode.OK;
        }
        finally
        {
            _resetEvent.Release();
        }
    }


    public Task ReleaseExclusive(ScopeContext context) => _leaseControl.ReleaseExclusive(context);

    public async ValueTask DisposeAsync() => await ReleaseExclusive(new ScopeContext(_logger));

    private async Task<Option> LoadDatabase(ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("graphMapStore-LoadDatabase");

        // Check if exclusive locked, not re-load database
        if (_leaseControl.IsExclusiveLocked) return StatusCode.OK;

        var dataETagOption = await _leaseControl.GetCurrentFileAccess().Get(context).ConfigureAwait(false);
        if (dataETagOption.IsError()) return dataETagOption.ToOptionStatus();

        _currentETag = dataETagOption.Return().ETag;

        GraphSerialization graphSerialization = dataETagOption.Return().ToObject<GraphSerialization>();
        GraphMap newMap = graphSerialization.FromSerialization();
        Interlocked.Exchange(ref _map, newMap);

        context.LogTrace("Read graph data file={mapDatabasePath}", GraphConstants.MapDatabasePath);
        return StatusCode.OK;
    }

    private async Task<Option<DataETag>> SaveDatabase(ScopeContext context)
    {
        context = context.With(_logger);
        _graphHostOption.ReadOnly.Assert(x => x == false, "Cannot set map when read-only");
        using var metric = context.LogDuration("graphMapStore-SaveDatabase");

        context.LogTrace("Writing graph data file={mapDatabasePath}", GraphConstants.MapDatabasePath);

        DataETag dataETag = _map
            .ToSerialization()
            .ToDataETag(_currentETag);

        var writeMapSerializer = await _leaseControl.GetCurrentFileAccess()
            .Set(dataETag, context)
            .ConfigureAwait(false);

        if (writeMapSerializer.IsError()) return writeMapSerializer.ToOptionStatus<DataETag>();

        string newETag = writeMapSerializer.Return();
        Interlocked.Exchange(ref _currentETag, newETag);

        context.LogTrace("Write graph data file={mapDatabasePath}, eTag={etag}", GraphConstants.MapDatabasePath, _currentETag);
        return dataETag;
    }
}
