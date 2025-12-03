using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class MemoryKeyStore : IKeyStore
{
    private readonly MemoryStore _memoryStore;
    private readonly ILogger<MemoryKeyStore> _logger;

    public MemoryKeyStore(MemoryStore memoryStore, ILogger<MemoryKeyStore> logger)
    {
        _memoryStore = memoryStore.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option<string>> Add(string path, DataETag data, ScopeContext context, TrxRecorder? recorder = null)
    {
        return _memoryStore.Add(path, data, context.With(_logger)).ToTaskResult();
    }

    public Task<Option<string>> Append(string path, DataETag data, ScopeContext context)
    {
        return _memoryStore.Append(path, data, null, context.With(_logger)).ToTaskResult();
    }

    public Task<Option> Delete(string path, ScopeContext context, TrxRecorder? recorder = null)
    {
        return _memoryStore.Delete(path, null, context.With(_logger)).ToTaskResult();
    }

    public Task<Option> DeleteFolder(string path, ScopeContext context)
    {
        return _memoryStore.DeleteFolder(path, context).ToTaskResult();
    }

    public Task<Option<DataETag>> Get(string path, ScopeContext context)
    {
        return _memoryStore.Get(path).ToTaskResult();
    }

    public Task<Option<string>> Set(string path, DataETag data, ScopeContext context, TrxRecorder? recorder = null)
    {
        return _memoryStore.Set(path, data, null, context.With(_logger)).ToTaskResult();
    }

    public Task<Option> Exists(string path, ScopeContext context)
    {
        return new Option(_memoryStore.Exist(path) ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option<IStorePathDetail>> GetDetails(string path, ScopeContext context)
    {
        return _memoryStore.GetDetail(path).ToTaskResult();
    }

    public Task<IReadOnlyList<IStorePathDetail>> Search(string pattern, ScopeContext context) => _memoryStore.Search(pattern).ToTaskResult();


    public Task<Option<string>> AcquireExclusiveLock(string path, bool breakLeaseIfExist, ScopeContext context)
    {
        return InternalAcquire(path, TimeSpan.FromSeconds(-1), true, context);
    }

    public Task<Option<string>> AcquireLease(string path, TimeSpan leaseDuration, ScopeContext context)
    {
        return InternalAcquire(path, leaseDuration, false, context);
    }

    public Task<Option> BreakLease(string path, ScopeContext context)
    {
        return _memoryStore.BreakLease(path, context.With(_logger)).ToTaskResult();
    }

    private async Task<Option<string>> InternalAcquire(string path, TimeSpan leaseDuration, bool breakLeaseIfExist, ScopeContext context)
    {
        context = context.With(_logger);
        if (breakLeaseIfExist) _memoryStore.BreakLease(path, context);

        DateTime dt = DateTime.UtcNow + TimeSpan.FromSeconds(5);

        while (DateTime.UtcNow < dt)
        {
            Option<LeaseRecord> lease = _memoryStore.AcquireLease(path, leaseDuration, context);
            if (lease.IsOk())
            {
                return lease.Return().LeaseId.ToOption();
            }

            if (lease.IsLocked())
            {
                var waitPeriod = TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(300));
                await Task.Delay(waitPeriod);
                context.LogInformation("Lease is locked, waiting for {waitPeriod}", waitPeriod);
                continue;
            }

            context.LogError("Failed to acquire lease, {statusCode}", lease.StatusCode);
        }

        return (StatusCode.Locked, "Timed out getting lease");
    }
}
