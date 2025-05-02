using Toolbox.Types;

namespace Toolbox.Store;

public interface IFileStore
{
    IFileAccess File(string path);

    Task<IReadOnlyList<IStorePathDetail>> Search(string pattern, ScopeContext context);
    Task<Option> DeleteFolder(string path, ScopeContext context);
}

public interface IFileReadWriteAccess
{
    Task<Option<string>> Append(DataETag data, ScopeContext context);
    Task<Option<DataETag>> Get(ScopeContext context);
    Task<Option<string>> Set(DataETag data, ScopeContext context);
}

public interface IFileAccess : IFileReadWriteAccess
{
    public string Path { get; }
    Task<Option<string>> Add(DataETag data, ScopeContext context);

    Task<Option<IStorePathDetail>> GetDetail(ScopeContext context);
    Task<Option> Delete(ScopeContext context);
    Task<Option> Exist(ScopeContext context);

    Task<Option<IFileLeasedAccess>> Acquire(TimeSpan leaseDuration, ScopeContext context);
    Task<Option<IFileLeasedAccess>> AcquireExclusive(bool breakLeaseIfExist, ScopeContext context);
    Task<Option> BreakLease(ScopeContext context);
}

public interface IFileLeasedAccess : IFileReadWriteAccess, IAsyncDisposable
{
    public string Path { get; }
    public string LeaseId { get; }

    Task<Option> Release(ScopeContext context);
}