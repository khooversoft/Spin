using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;


public class GraphMapStore : IAsyncDisposable
{
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly ILogger<GraphMapStore> _logger;
    private readonly GraphHostOption _graphHostOption;
    private readonly IGraphMapFactory _graphMapFactory;
    private string? _currentETag;
    private GraphMap? _map;
    private GraphLeaseControl _leaseControl;

    public GraphMapStore(GraphLeaseControl leaseControl, IGraphMapFactory graphMapFactory, GraphHostOption graphHostOption, ILogger<GraphMapStore> logger)
    {
        _leaseControl = leaseControl.NotNull();
        _graphHostOption = graphHostOption.NotNull();
        _graphMapFactory = graphMapFactory.NotNull();
        _logger = logger.NotNull();
    }


    public GraphMap GetMapReference() => _map.NotNull("Graph not loaded");

    public async Task<Option<IFileLeasedAccess>> AcquireLease(ScopeContext context)
    {
        var result = await _leaseControl.Acquire(context);
        if (result.IsError()) return result;
        await using var scope = result.Return();

        var loadDatabase = await LoadDatabase(context).ConfigureAwait(false);
        return result;
    }

    public Task<Option> ReleaseLease(ScopeContext context) => _leaseControl.Release(context);

    public async Task<Option> CheckpointMap(ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        using var metric = context.LogDuration("graphMapStore-CheckpointMap");

        try
        {
            var setResult = await SaveDatabase(context).ConfigureAwait(false);
            return setResult.ToOptionStatus();
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task SetMap(GraphMap map, ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        context.LogDebug("Setting database map to provided value");

        try
        {
            _map = _graphMapFactory.Create(map.Nodes, map.Edges);
            (await SaveDatabase(context).ConfigureAwait(false)).ThrowOnError();
            _map.UpdateCounters();
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
        if (_map != null && _leaseControl.IsExclusiveLocked)
        {
            context.LogWarning("Graph database already loaded, exclusive lock, no reload");
            return StatusCode.OK;
        }

        IFileReadWriteAccess reader = _leaseControl.GetCurrentFileAccess();

        context.LogDebug("Loading graph data, leaseId={leaseId}", reader.GetLeaseId());
        var dataETagOption = await reader.Get(context).ConfigureAwait(false);
        if (dataETagOption.IsError()) return dataETagOption.ToOptionStatus();

        _currentETag = dataETagOption.Return().ETag;
        var dataETag = dataETagOption.Return();

        GraphMap newMap = dataETag.Data.Length switch
        {
            0 => _graphMapFactory.Create(),
            _ => _graphMapFactory.Create(dataETag)
        };

        Interlocked.Exchange(ref _map, newMap);

        if (dataETag.Data.Length == 0)
        {
            var updateOption = await SaveDatabase(context).ConfigureAwait(false);
            if (updateOption.IsError()) return updateOption.ToOptionStatus();
        }

        _map.UpdateCounters();

        context.LogInformation("Loaded graph data file={mapDatabasePath}", GraphConstants.MapDatabasePath);
        return StatusCode.OK;
    }

    private async Task<Option<DataETag>> SaveDatabase(ScopeContext context)
    {
        context = context.With(_logger);
        _graphHostOption.ReadOnly.Assert(x => x == false, "Cannot set map when read-only");
        using var metric = context.LogDuration("graphMapStore-SaveDatabase");

        IFileReadWriteAccess writer = _leaseControl.GetCurrentFileAccess();
        if (writer.GetLeaseId() == null)
        {
            context.LogCritical("No lease for writing database file");
            throw new InvalidOperationException("No lease for writing database file");
        }

        context.LogDebug("Writing graph data file={mapDatabasePath}, leaseId={leaseId}", GraphConstants.MapDatabasePath, writer.GetLeaseId());

        DataETag dataETag = _map.NotNull("Graph not loaded")
            .ToSerialization()
            .ToDataETag(_currentETag);

        var saveOption = await writer.Set(dataETag, context).ConfigureAwait(false);
        if (saveOption.IsError()) return saveOption.LogStatus(context, "Failed to save database").ToOptionStatus<DataETag>();

        string newETag = saveOption.Return();
        Interlocked.Exchange(ref _currentETag, newETag);
        _map.UpdateCounters();

        context.LogDebug("Write graph data file={mapDatabasePath}, eTag={etag}", GraphConstants.MapDatabasePath, _currentETag);
        return dataETag;
    }
}
