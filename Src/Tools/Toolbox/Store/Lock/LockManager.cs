using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class LockManager : IAsyncDisposable
{
    private ConcurrentDictionary<string, LockDetail> _lockMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly IFileStore _fileStore;
    private readonly ILogger<LockManager> _logger;
    private readonly static TimeSpan _defaultLockDuration = TimeSpan.FromSeconds(60);

    public LockManager(IFileStore fileStore, ILogger<LockManager> logger)
    {
        _fileStore = fileStore.NotNull();
        _logger = logger.NotNull();
    }

    public IFileReadWriteAccess GetReadWriteAccess(string path, ScopeContext context)
    {
        context = context.With(_logger);
        path.NotEmpty();
        context.LogDebug("Check if path is locked, path={path}", path);

        LockDetail? lockDetail = Lookup(path, context);
        if (lockDetail == null) return _fileStore.File(path);

        context.LogDebug("Using locked file access for path={path}", path);
        return lockDetail.FileLeasedAccess;
    }

    public async Task<Option> ReleaseLock(string path, ScopeContext context)
    {
        path.NotEmpty();
        context.LogDebug("Release lock, path={path}", path);

        LockDetail? lockDetail = Lookup(path, context);
        if (lockDetail == null) return StatusCode.OK;

        _lockMap.TryRemove(path, out _);

        var result = await lockDetail.FileLeasedAccess.Release(context).ConfigureAwait(false);
        result.LogStatus(context, "Released lock for path={path}", [path]);

        return StatusCode.OK;
    }

    public async Task<Option> ProcessLock(string path, LockMode lockMode, ScopeContext context)
    {
        path.NotEmpty();
        context.LogDebug("Processing lock, path={path}, lockMode={lockMode}", path, lockMode);

        LockDetail? lockDetail = Lookup(path, context);
        if (lockDetail != null) return StatusCode.OK;

        switch (lockMode)
        {
            case LockMode.Shared:
                await Acquire(path, context).ConfigureAwait(false);
                break;

            case LockMode.Exclusive:
                await AcquireExclusive(path, context).ConfigureAwait(false);
                break;

            default: throw new InvalidOperationException($"Unknown lock mode '{lockMode}' for path '{path}'");
        }

        return StatusCode.OK;
    }

    public async Task<Option<bool>> IsLocked(string path, ScopeContext context)
    {
        path.NotEmpty();
        context = context.With(_logger);
        context.LogDebug("Checking if path is locked, path={path}", path);

        var fileDetailOption = await _fileStore.File(path).GetDetails(context);
        if (fileDetailOption.IsNotFound())
        {
            _lockMap.Remove(path, out _);
            return fileDetailOption.ToOptionStatus<bool>();
        }

        var fileDetail = fileDetailOption.Return();

        if (fileDetail.LeaseStatus == LeaseStatus.Locked)
        {
            context.LogDebug("Path is locked, path={path}, leaseStatus={leaseStatus}", path, fileDetail.LeaseStatus);
            return true;
        }

        context.LogDebug("Path is not locked, path={path}, leaseStatus={leaseStatus}", path, fileDetail.LeaseStatus);
        return false;
    }

    private async Task<Option> Acquire(string path, ScopeContext context)
    {
        context.LogDebug("Acquiring shared lock for path={path}", path);
        var lockOption = await _fileStore.File(path).AcquireLease(_defaultLockDuration, context).ConfigureAwait(false);

        if (lockOption.IsError()) return lockOption.LogStatus(context, "Failed to acquire lock for path={path}", [path]).ToOptionStatus();

        SetDetail(new LockDetail(path, lockOption.Return(), LockMode.Shared, _defaultLockDuration));
        return StatusCode.OK;
    }

    private async Task<Option> AcquireExclusive(string path, ScopeContext context)
    {
        context.LogDebug("Acquiring exclusive lock for path={path}", path);

        var lockOption = await _fileStore.File(path).AcquireExclusiveLease(true, context).ConfigureAwait(false);
        if (lockOption.IsError()) return lockOption.LogStatus(context, "Failed to acquire exclusive lock for path={path}", [path]).ToOptionStatus();

        SetDetail(new LockDetail(path, lockOption.Return(), LockMode.Exclusive, TimeSpan.MaxValue));
        return StatusCode.OK;
    }

    private LockDetail? Lookup(string path, ScopeContext context)
    {
        path.NotEmpty();

        if (!_lockMap.TryGetValue(path, out LockDetail? detail)) return null;

        if (!IsValid(detail))
        {
            _logger.LogWarning("Lock for file {File} has expired, removing from lock map", detail.Path);
            _lockMap.TryRemove(path, out _);
            return null;
        }

        return detail;
    }

    private void SetDetail(LockDetail detail) => _lockMap.AddOrUpdate(detail.Key, detail, (key, oldValue) => detail);

    public async ValueTask DisposeAsync()
    {
        var context = _logger.ToScopeContext();
        context.LogDebug("Disposing LockDetailCollection, clearing all locks");

        var release = _lockMap.Values
            .Where(x => IsValid(x))
            .ToArray();

        _lockMap.Clear();

        foreach (var item in release)
        {
            context.LogDebug("Releasing lock for file {File}", item.Path);
            await item.FileLeasedAccess.DisposeAsync().ConfigureAwait(false);
        }
    }

    private bool IsValid(LockDetail detail) => detail.LockMode switch
    {
        LockMode.Exclusive => true,
        LockMode.Shared => detail.AcquiredDate + _defaultLockDuration > DateTime.UtcNow.AddMinutes(-2),

        _ => throw new InvalidOperationException($"Unknown lock mode '{detail.LockMode}' for path '{detail.Path}'")
    };
}