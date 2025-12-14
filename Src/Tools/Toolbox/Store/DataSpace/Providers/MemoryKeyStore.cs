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

    public Task<Option<string>> Add(string key, DataETag data, ScopeContext context, TrxRecorder? recorder = null)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return _memoryStore.Add(key, data, context.With(_logger), recorder).ToTaskResult();
    }

    public Task<Option<string>> Append(string key, DataETag data, ScopeContext context, string? leaseId = null)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);
        return _memoryStore.Append(key, data, leaseId, context).ToTaskResult();
    }

    public Task<Option> Delete(string key, ScopeContext context, TrxRecorder? recorder = null, string? leaseId = null)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return _memoryStore.Delete(key, leaseId, context.With(_logger), recorder).ToTaskResult();
    }

    public Task<Option> DeleteFolder(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return _memoryStore.DeleteFolder(key, context).ToTaskResult();
    }

    public Task<Option<DataETag>> Get(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return _memoryStore.Get(key).ToTaskResult();
    }

    public Task<Option<string>> Set(string key, DataETag data, ScopeContext context, TrxRecorder? recorder = null, string? leaseId = null)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return _memoryStore.Set(key, data, leaseId, context.With(_logger), recorder).ToTaskResult();
    }

    public Task<Option> Exists(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return new Option(_memoryStore.Exist(key) ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();
    }

    public Task<Option<StorePathDetail>> GetDetails(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return _memoryStore.GetDetail(key).ToTaskResult();
    }

    public Task<IReadOnlyList<StorePathDetail>> Search(string pattern, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Search pattern={pattern}", pattern);
        return _memoryStore.Search(pattern).ToTaskResult();
    }

    public Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return InternalAcquire(key, TimeSpan.FromSeconds(-1), true, context);
    }

    public Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return InternalAcquire(key, leaseDuration, false, context);
    }

    public Task<Option> BreakLease(string key, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Appending key={key}", key);

        return _memoryStore.BreakLease(key, context).ToTaskResult();
    }

    public Task<Option> Release(string leaseId, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Release leaseId={leaseId}", leaseId);
        return _memoryStore.ReleaseLease(leaseId, context).ToTaskResult();
    }

    private async Task<Option<string>> InternalAcquire(string path, TimeSpan leaseDuration, bool breakLeaseIfExist, ScopeContext context)
    {
        context = context.With(_logger);
        if (breakLeaseIfExist) _memoryStore.BreakLease(path, context);
        context.LogDebug("Acquiring path={path}, duration={duration}, breakLeaseIfExist={breakLeaseIfExist}", path, leaseDuration, breakLeaseIfExist);

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
