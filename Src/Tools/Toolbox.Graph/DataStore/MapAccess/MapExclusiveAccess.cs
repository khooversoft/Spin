using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class MapExclusiveAccess : IGraphMapAccess
{
    private readonly ILogger<MapExclusiveAccess> _logger;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly GraphMapCounter _mapCounters;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGraphFileStore _graphFileStore;
    private IFileLeasedAccess? _fileAccess;

    public MapExclusiveAccess(IGraphFileStore graphFileStore, GraphMapCounter mapCounters, IServiceProvider serviceProvider, ILogger<MapExclusiveAccess> logger)
    {
        _graphFileStore = graphFileStore.NotNull();
        _serviceProvider = serviceProvider.NotNull();
        _mapCounters = mapCounters.NotNull();
        _logger = logger.NotNull();
    }

    public bool IsShareMode => false;

    public Task<IGraphMapAccessScope> CreateScope(ScopeContext context)
    {
        _fileAccess.NotNull("File access is not started");

        IGraphMapAccessScope scope = ActivatorUtilities.CreateInstance<MapScopeAccess>(_serviceProvider, _fileAccess, () => Task.CompletedTask);
        return scope.ToTaskResult();
    }

    public async ValueTask DisposeAsync()
    {
        var context = _logger.ToScopeContext();
        context = context.With(_logger);
        context.LogDebug("Disposing exclusive map access");
        await Stop(context);
    }

    public async Task<Option> Start(ScopeContext context)
    {
        context = context.With(_logger);
        await _lock.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            if (_fileAccess != null)
            {
                context.LogCritical("Map access is started, exclusive lease already acquired, leaseId={leaseId}", _fileAccess.LeaseId);
                throw new InvalidOperationException($"Map access has already been started, leaseId={_fileAccess.LeaseId}");
            }

            _mapCounters.Leases.ActiveExclusive.Record(1);
            _fileAccess = await AcquireExclusive(context).ConfigureAwait(false);
            context.LogWarning("Exclusive lease acquired, leaseId={leaseId}", _fileAccess.LeaseId);

            return StatusCode.OK;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Option> Stop(ScopeContext context)
    {
        context = context.With(_logger);
        await _lock.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            var current = Interlocked.Exchange(ref _fileAccess, null);
            if (current == null) return StatusCode.OK;

            await current.Release(context).ConfigureAwait(false);
            _mapCounters.Leases.ActiveExclusive.Record(0);
            _mapCounters.Leases.Release.Add();
            context.LogInformation("Exclusive lease released, leaseId={leaseId}", current.LeaseId);

            return StatusCode.OK;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IFileLeasedAccess> AcquireExclusive(ScopeContext context)
    {
        using var metric = context.LogDuration("GraphMapExclusiveAccess-AcquireExclusive");
        context.LogDebug("Acquiring exclusive lease");

        var leaseOption = await _graphFileStore.File(GraphConstants.MapDatabasePath).AcquireExclusive(true, context).ConfigureAwait(false);
        if (leaseOption.IsOk()) return leaseOption.Return();

        if (leaseOption.IsLocked())
        {
            var lockOption = await _graphFileStore.File(GraphConstants.MapDatabasePath).BreakLease(context).ConfigureAwait(false);
            if (lockOption.IsError())
            {
                context.LogCritical("Failed to release exclusive lock, error={error}", lockOption.Error);
                throw new InvalidOperationException($"Failed to release exclusive lock, error={lockOption.Error}");
            }
        }

        leaseOption = await _graphFileStore.File(GraphConstants.MapDatabasePath).AcquireExclusive(true, context).ConfigureAwait(false);
        if (leaseOption.IsError())
        {
            context.LogCritical("Failed to acquire exclusive lock, error={error}", leaseOption.Error);
            throw new InvalidOperationException($"Failed to release exclusive lock, error={leaseOption.Error}");
        }

        _mapCounters.Leases.Acquire.Add();
        return leaseOption.Return();
    }
}
