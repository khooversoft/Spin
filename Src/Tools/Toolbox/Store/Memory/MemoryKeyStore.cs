using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
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

    public Task<Option<string>> Add(string key, DataETag data) => _memoryStore.Add(key, data).ToTaskResult();
    public Task<Option<string>> Append(string key, DataETag data, string? leaseId = null) => _memoryStore.Append(key, data, leaseId).ToTaskResult();
    public Task<Option> Delete(string key, string? leaseId = null) => _memoryStore.Delete(key, leaseId).ToTaskResult();
    public Task<Option> DeleteFolder(string key) => _memoryStore.DeleteFolder(key).ToTaskResult();
    public Task<Option<DataETag>> Get(string key) => _memoryStore.Get(key).ToTaskResult();
    public Task<Option<string>> Set(string key, DataETag data, string? leaseId = null) => _memoryStore.Set(key, data, leaseId).ToTaskResult();
    public Task<Option> Exists(string key) => new Option(_memoryStore.Exist(key) ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();
    public Task<Option<StorePathDetail>> GetDetails(string key) => _memoryStore.GetDetail(key).ToTaskResult();
    public Task<IReadOnlyList<StorePathDetail>> Search(string pattern, int index = 0, int size = -1) => _memoryStore.Search(pattern, index, size).ToTaskResult();
    public Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist) => InternalAcquire(key, TimeSpan.FromSeconds(-1), true);
    public Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration) => InternalAcquire(key, leaseDuration, false);
    public Task<Option> BreakLease(string key) => _memoryStore.BreakLease(key).ToTaskResult();
    public Task<Option> ReleaseLease(string key, string leaseId) => _memoryStore.ReleaseLease(key, leaseId).ToTaskResult();
    public Task<Option> RenewLease(string key, string leaseId) => _memoryStore.RenewLease(key, leaseId).ToTaskResult();

    private async Task<Option<string>> InternalAcquire(string path, TimeSpan leaseDuration, bool breakLeaseIfExist)
    {
        if (breakLeaseIfExist) _memoryStore.BreakLease(path);
        _logger.LogDebug("Acquiring path={path}, duration={duration}, breakLeaseIfExist={breakLeaseIfExist}", path, leaseDuration, breakLeaseIfExist);

        DateTime dt = DateTime.UtcNow + TimeSpan.FromSeconds(5);

        while (DateTime.UtcNow < dt)
        {
            Option<LeaseRecord> lease = _memoryStore.AcquireLease(path, leaseDuration);
            if (lease.IsOk())
            {
                return lease.Return().LeaseId.ToOption();
            }

            if (lease.IsLocked())
            {
                var waitPeriod = TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(300));
                await Task.Delay(waitPeriod);
                _logger.LogInformation("Lease is locked, waiting for {waitPeriod}", waitPeriod);
                continue;
            }

            _logger.LogError("Failed to acquire lease, {statusCode}", lease.StatusCode);
        }

        return (StatusCode.Locked, "Timed out getting lease");
    }
}
