using Microsoft.Extensions.Logging;
using Toolbox.Types;

namespace Toolbox.Store;

public partial class KeySpace
{
    public Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("AcquireExclusiveLock path={path}", path);

        return _keyStore.AcquireExclusiveLock(path, breakLeaseIfExist);
    }

    public Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("AcquireLease path={path}", path);

        return _keyStore.AcquireLease(path, leaseDuration);
    }

    public Task<Option> BreakLease(string key)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("BreakLease path={path}", path);

        return _keyStore.BreakLease(path);
    }

    public Task<Option> ReleaseLease(string key, string leaseId)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("Release path={path}, leaseId={leaseId}", path, leaseId);
        return _keyStore.ReleaseLease(path, leaseId);
    }

    public Task<Option> RenewLease(string key, string leaseId)
    {
        var path = _keyPathStrategy.BuildPath(key);
        _logger.LogDebug("RenewLease path={path}, leaseId={leaseId}", path, leaseId);
        return _keyStore.RenewLease(path, leaseId);
    }
}
