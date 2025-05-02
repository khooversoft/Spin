using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class InMemoryFileAccess : IFileAccess
{
    private readonly MemoryStore _memoryStore;
    private readonly ILogger _logger;

    internal InMemoryFileAccess(string path, MemoryStore memoryStore, ILogger logger)
    {
        Path = path.NotEmpty();
        _memoryStore = memoryStore.NotNull();
        _logger = logger.NotNull();
    }

    public string Path { get; }

    public Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, ScopeContext context) => InternalAcquire(leaseDuration, false, context);
    public Task<Option<IFileLeasedAccess>> AcquireExclusive(bool breakLeaseIfExist, ScopeContext context) => InternalAcquire(TimeSpan.FromSeconds(-1), true, context);

    public Task<Option<string>> Add(DataETag data, ScopeContext context) => _memoryStore.Add(Path, data, context.With(_logger)).ToTaskResult();
    public Task<Option<string>> Append(DataETag data, ScopeContext context) => _memoryStore.Append(Path, data, null, context.With(_logger)).ToTaskResult();
    public Task<Option> BreakLease(ScopeContext context) => _memoryStore.BreakLease(Path, context.With(_logger)).ToTaskResult();
    public Task<Option> Delete(ScopeContext context) => _memoryStore.Delete(Path, null, context.With(_logger)).ToTaskResult();
    public Task<Option> Exist(ScopeContext context) => new Option(_memoryStore.Exist(Path) ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();
    public Task<Option<DataETag>> Get(ScopeContext context) => _memoryStore.Get(Path).ToTaskResult();
    public Task<Option<IStorePathDetail>> GetDetail(ScopeContext context) => _memoryStore.GetDetail(Path).ToTaskResult();
    public Task<Option<string>> Set(DataETag data, ScopeContext context) => _memoryStore.Set(Path, data, null, context.With(_logger)).ToTaskResult();

    private Task<Option<IFileLeasedAccess>> InternalAcquire(TimeSpan leaseDuration, bool breakLeaseIfExist, ScopeContext context)
    {
        if (breakLeaseIfExist) _memoryStore.BreakLease(Path, context);

        Option<LeaseRecord> lease = _memoryStore.AcquireLease(Path, leaseDuration, context.With(_logger));
        if (lease.IsError()) return lease.ToOptionStatus<IFileLeasedAccess>().ToTaskResult();

        IFileLeasedAccess access = new InMemoryLeasedAccess(lease.Return(), _memoryStore, _logger);
        return access.ToOption().ToTaskResult();
    }
}
