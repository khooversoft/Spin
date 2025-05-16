using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphLeaseControl
{
    private const string _leaseAlreadyPresentText = "LeaseAlreadyPresent";

    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly IGraphStore _graphStore;
    private readonly ILogger<GraphLeaseControl> _logger;
    private readonly GraphHostOption _graphHostOption;
    private readonly LeaseCounter _leaseCounter;
    private IFileLeasedAccess? _exclusiveLock;
    private IFileLeasedAccess? _scopeLock;

    public GraphLeaseControl(IGraphStore graphStore, GraphMapCounter mapCounters, GraphHostOption graphHostOption, ILogger<GraphLeaseControl> logger)
    {
        _graphStore = graphStore.NotNull();
        _graphHostOption = graphHostOption.NotNull();
        _leaseCounter = mapCounters.NotNull().Leases;
        _logger = logger.NotNull();
    }
    public bool IsExclusiveLocked => _exclusiveLock != null;

    public IFileReadWriteAccess GetCurrentFileAccess() => _exclusiveLock switch
    {
        IFileLeasedAccess v => v,
        null => _scopeLock switch
        {
            IFileLeasedAccess v => v,
            _ => _graphStore.File(GraphConstants.MapDatabasePath),
        }
    };

    public async Task<Option<IFileLeasedAccess>> Acquire(ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            if (_graphHostOption.ShareMode)
            {
                var scope = await AcquireScope(context).ConfigureAwait(false);
                return scope;
            }

            return await AcquireExclusive(context).ConfigureAwait(false);
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<Option> Release(ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        using var metric = context.LogDuration("ReleaseExclusive - release lock");

        try
        {
            if (_exclusiveLock != null) return StatusCode.OK;

            var current = Interlocked.Exchange(ref _scopeLock, null);
            if (current == null) return StatusCode.OK;

            _leaseCounter.ActiveAcquire.Record(0);
            _leaseCounter.Release.Add();

            var result = await current.NotNull().Release(context).ConfigureAwait(false);
            context.LogInformation("Lease released");
            return result;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<Option> ReleaseExclusive(ScopeContext context)
    {
        context = context.With(_logger);
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        using var metric = context.LogDuration("ReleaseExclusive - release exclusive lock");

        try
        {
            var currentScope = Interlocked.Exchange(ref _scopeLock, null);
            if (currentScope != null) await currentScope.Release(context).ConfigureAwait(false);

            var currentLock = Interlocked.Exchange(ref _exclusiveLock, null);
            if (currentLock != null)
            {
                await currentLock.Release(context).ConfigureAwait(false);
                _leaseCounter.ActiveExclusive.Record(0);
                context.LogInformation("Exclusive lock released");
            }

            return StatusCode.OK;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    private async Task<Option<IFileLeasedAccess>> AcquireScope(ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("Acquire Scope - acquire time limited lock for database file");

        // If exclusive lock, just return a no-op release
        if (_exclusiveLock != null) return new ScopedWriteAccess(_exclusiveLock, () => Task.CompletedTask);
        if (_scopeLock != null) await _scopeLock.Release(context).ConfigureAwait(false);
        _scopeLock = null;

        var leaseOption = await _graphStore.File(GraphConstants.MapDatabasePath).Acquire(TimeSpan.FromSeconds(30), context).ConfigureAwait(false);
        if (leaseOption.IsError()) return leaseOption;

        _scopeLock = leaseOption.Return();
        var scopedWriteAccess = new ScopedWriteAccess(_scopeLock, () => Release(context));

        _leaseCounter.ActiveAcquire.Record(1);
        _leaseCounter.Acquire.Add();
        context.LogInformation("Lease acquired");

        return scopedWriteAccess;

    }

    private async Task<Option<IFileLeasedAccess>> AcquireExclusive(ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("graphMapStore-AcquireExclusive");

        if (_exclusiveLock != null) return new ScopedWriteAccess(_exclusiveLock, () => Task.CompletedTask);

        int loopCount = 2;
        Option<IFileLeasedAccess> leaseOption = null!;

        while (loopCount-- > 0)
        {
            leaseOption = await _graphStore.File(GraphConstants.MapDatabasePath).AcquireExclusive(true, context).ConfigureAwait(false);
            if (leaseOption.IsOk())
            {
                _exclusiveLock = leaseOption.Return();
                _leaseCounter.ActiveExclusive.Record(1);
                context.LogInformation("Exclusive lock acquired");

                var scopedWriteAccess = new ScopedWriteAccess(_exclusiveLock, () => Task.CompletedTask);
                return scopedWriteAccess;
            }

            if (leaseOption.IsLocked())
            {
                var releaseOption = await ReleaseExclusive(context).ConfigureAwait(false);
                if (releaseOption.IsError())
                {
                    context.LogError(releaseOption.Error, "Failed to release exclusive lock");
                    return releaseOption.ToOptionStatus<IFileLeasedAccess>();
                }

                continue;
            }

            // return error
            return leaseOption;
        }

        throw new UnreachableException("Flow failed");
    }

    public class ScopedWriteAccess : IFileLeasedAccess, IAsyncDisposable
    {
        private readonly IFileLeasedAccess _writeAccess;
        private readonly Func<Task> _release;

        internal ScopedWriteAccess(IFileLeasedAccess writeAccess, Func<Task> release)
        {
            _writeAccess = writeAccess.NotNull();
            _release = release.NotNull();
        }

        public string Path => _writeAccess.Path;
        public string LeaseId => _writeAccess.LeaseId;

        public Task<Option<string>> Append(DataETag data, ScopeContext context) => _writeAccess.Append(data, context);
        public Task<Option<DataETag>> Get(ScopeContext context) => _writeAccess.Get(context);
        public Task<Option<string>> Set(DataETag data, ScopeContext context) => _writeAccess.Set(data, context);
        public async ValueTask DisposeAsync() => await _release();

        public async Task<Option> Release(ScopeContext context)
        {
            await _release();
            return StatusCode.OK;
        }
    }
}
