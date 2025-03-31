using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class InMemoryLeasedAccess : IFileLeasedAccess
{
    private readonly LeaseRecord _leaseRecord;
    private readonly MemoryStore _memoryStore;
    private readonly ILogger _logger;

    internal InMemoryLeasedAccess(LeaseRecord leaseRecord, MemoryStore memoryStore, ILogger logger)
    {
        _leaseRecord = leaseRecord.NotNull();
        _memoryStore = memoryStore.NotNull();
        _logger = logger;
    }

    public string Path => _leaseRecord.Path;
    public string LeaseId => _leaseRecord.LeaseId;

    public Task<Option<string>> Append(DataETag data, ScopeContext context) => _memoryStore.Append(Path, data, LeaseId, context).ToTaskResult();

    public ValueTask DisposeAsync()
    {
        _memoryStore.ReleaseLease(LeaseId, new ScopeContext(_logger));
        return ValueTask.CompletedTask;
    }

    public Task<Option<DataETag>> Get(ScopeContext context) => _memoryStore.Get(Path).ToTaskResult();
    public Task<Option> Release(ScopeContext context) => _memoryStore.ReleaseLease(LeaseId, context).ToTaskResult();
    public Task<Option<string>> Set(DataETag data, ScopeContext context) => _memoryStore.Set(Path, data, LeaseId, context).ToTaskResult();
}
