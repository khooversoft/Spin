using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class InMemoryStoreLeaseControl : IFileLeaseControl
{
    private ILogger _logger;
    private readonly ConcurrentDictionary<string, LeaseRecord> _leaseStore = new(StringComparer.OrdinalIgnoreCase);
    private readonly InMemoryStoreControl _storeControl;
    private readonly string _path;

    internal InMemoryStoreLeaseControl(string path, InMemoryStoreControl storeControl, ILogger logger)
    {
        _path = path.NotEmpty();
        _storeControl = storeControl.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, TimeSpan timeOut, ScopeContext context)
    {
        return AcquireInternal(_path, leaseDuration, timeOut, context).ToTaskResult();
    }

    public Task<Option<IFileLeasedAccess>> AcquireExclusive(TimeSpan timeOut, ScopeContext context)
    {
        return AcquireInternal(_path, TimeSpan.FromSeconds(-1), timeOut, context).ToTaskResult();
    }

    public Task<Option<IFileLeasedAccess>> Break(TimeSpan leaseDuration, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option> Clear(string path, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    internal bool IsLeased(string path) => _leaseStore.TryGetValue(path, out var lease) && lease.IsLeaseValid();
    internal void Release(string leaseId)

    private Option<IFileLeasedAccess> AcquireInternal(string path, TimeSpan leaseDuration, TimeSpan timeOut, ScopeContext context)
    {
        leaseDuration = leaseDuration.Assert(x => x > TimeSpan.Zero, "Duration must be greater than zero");
        timeOut = timeOut.Assert(x => x > TimeSpan.Zero, "Duration must be greater than zero");
        context = context.With(_logger);

        LeaseRecord? lease = null;
        _leaseStore.AddOrUpdate(path, createLease, updateLease);
        if (lease == null) return (StatusCode.Conflict, "Lease already exist");
        context.LogTrace("Acquired lease for path={path}, leaseId={leaseId}, expiration={expiration}", path, lease.LeaseId, lease.Expiration);

        var result = new InMemoryLeasedAccess(path, lease.LeaseId, _storeControl, this);
        return result;


        LeaseRecord createLease(string path) => lease = new LeaseRecord(path, leaseDuration);

        LeaseRecord updateLease(string path, LeaseRecord lease) => (lease.Expiration < DateTimeOffset.UtcNow) switch
        {
            true => createLease(path),
            false => lease,
        };
    }
}
