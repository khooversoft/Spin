using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

/// <summary>
/// Lease management for the Graph Map File
/// 
/// If exclusive lock is acquired
///   Create new WriteAccess that uses the exclusive log for writes, and push on stack
///   Release will just pop the stack, the exclusive lock will NOT be released
///   
/// If no exclusive lock has been acquired
///   Create new WriteAccess after a limited lock has been acquired, and push on stack
///   Release will pop the stack and release the limited lock
/// 
/// </summary>
public class GraphLeaseControl
{
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

    public IFileReadWriteAccess GetCurrentFileAccess() => _exclusiveLock switch
    {
        IFileLeasedAccess v => v,
        null => _scopeLock switch
        {
            IFileLeasedAccess v => v,
            _ => _graphStore.File(GraphConstants.MapDatabasePath),
        }
    };

    public bool IsExclusiveLocked => _exclusiveLock != null;

    public async Task<Option> AcquireExclusive(ScopeContext context)
    {
        context = context.With(_logger);

        if (_graphHostOption.ShareMode)
        {
            context.LogWarning("Graph is in share mode, cannot acquire exclusive lock");
            return StatusCode.OK;
        }

        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            if (_exclusiveLock != null) return (StatusCode.Conflict, "Exclusive lock already acquired");

            var leaseOption = await _graphStore
                .File(GraphConstants.MapDatabasePath)
                .AcquireExclusive(context)
                .ConfigureAwait(false);

            if (leaseOption.IsError()) return leaseOption.ToOptionStatus();

            _exclusiveLock = leaseOption.Return();
            _leaseCounter.ActiveExclusive.Record(1);
            context.LogTrace("Exclusive lock acquired");
            return StatusCode.OK;
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

        try
        {
            var currentScope = Interlocked.Exchange(ref _scopeLock, null);
            if (currentScope != null) await currentScope.Release(context).ConfigureAwait(false);

            var currentLock = Interlocked.Exchange(ref _exclusiveLock, null);
            if (currentLock != null) await currentLock.Release(context).ConfigureAwait(false);

            _leaseCounter.ActiveExclusive.Record(0);
            context.LogTrace("Exclusive lock released");
            return StatusCode.OK;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<Option<IAsyncDisposable>> AcquireScope(ScopeContext context)
    {
        context = context.With(_logger);
        using var metric = context.LogDuration("Acquire Scope - acquire time limited lock for database file");

        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            // If exclusive lock, just return a no-op release
            if (_exclusiveLock != null) return new ScopedWriteAccess(_exclusiveLock, () => Task.CompletedTask);
            if (_scopeLock != null) await _scopeLock.Release(context).ConfigureAwait(false);
            _scopeLock = null;

            var leaseOption = await _graphStore.File(GraphConstants.MapDatabasePath).Acquire(TimeSpan.FromSeconds(30), context).ConfigureAwait(false);
            if (leaseOption.IsError()) return leaseOption.ToOptionStatus<IAsyncDisposable>();

            _scopeLock = leaseOption.Return();
            var scopedWriteAccess = new ScopedWriteAccess(_scopeLock, () => ReleaseScope(context));

            _leaseCounter.ActiveAcquire.Record(1);
            _leaseCounter.Acquire.Add();
            context.LogTrace("Lock acquired");

            return scopedWriteAccess;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    private async Task<Option> ReleaseScope(ScopeContext context)
    {
        await _resetEvent.WaitAsync(context.CancellationToken).ConfigureAwait(false);

        try
        {
            if (_exclusiveLock != null) return StatusCode.OK;

            var current = Interlocked.Exchange(ref _scopeLock, null);
            if (current == null) return StatusCode.OK;

            context.LogTrace("Lease released");
            _leaseCounter.ActiveAcquire.Record(0);
            _leaseCounter.Release.Add();
            context.LogTrace("Lock released");

            return await current.NotNull().Release(context).ConfigureAwait(false);
        }
        finally
        {
            _resetEvent.Release();
        }
    }


    public class ScopedWriteAccess : IFileLeasedAccess
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
