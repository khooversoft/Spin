using Toolbox.Types;

namespace Toolbox.Store;

public interface IFileStore
{
    IFileAccess File(string path);

    Task<IReadOnlyList<IStorePathDetail>> Search(string pattern, ScopeContext context);
    Task<Option> DeleteFolder(string path, ScopeContext context);
}

public interface IFileAccess
{
    public string Path { get; }
    Task<Option<string>> Add(DataETag data, ScopeContext context);
    Task<Option> Append(DataETag data, ScopeContext context);
    Task<Option<DataETag>> Get(ScopeContext context);
    Task<Option<string>> Set(DataETag data, ScopeContext context);

    Task<Option<IStorePathDetail>> GetDetail(ScopeContext context);
    Task<Option> Delete(ScopeContext context);
    Task<Option> Exist(ScopeContext context);

    Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, ScopeContext context);
    Task<Option<IFileLeasedAccess>> AcquireExclusive(ScopeContext context);
    Task<Option> BreakLease(ScopeContext context);
    Task<Option> ClearLease(ScopeContext context);
}

public interface IFileLeasedAccess : IAsyncDisposable
{
    public string Path { get; }
    public string LeaseId { get; }
    public DateTime DateAcquired { get; }
    public TimeSpan Elapsed => DateTime.UtcNow - DateAcquired;
    public bool ShouldRenew => Elapsed > TimeSpan.FromSeconds(30);

    Task<Option> Append(DataETag data, ScopeContext context);
    Task<Option<DataETag>> Get(ScopeContext context);
    Task<Option<string>> Set(DataETag data, ScopeContext context);
    Task<Option> Renew(ScopeContext context);
    Task<Option> Release(ScopeContext context);
}