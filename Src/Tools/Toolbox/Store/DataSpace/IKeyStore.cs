using Toolbox.Types;

namespace Toolbox.Store;

public interface IKeyStore
{
    Task<Option<string>> Add(string key, DataETag data);
    Task<Option<string>> Append(string key, DataETag data, string? leaseId = null);
    Task<Option> Delete(string key, string? leaseId = null);
    Task<Option<DataETag>> Get(string key);
    Task<Option<string>> Set(string key, DataETag data, string? leaseId = null);

    Task<Option> DeleteFolder(string key);
    Task<Option> Exists(string key);
    Task<Option<StorePathDetail>> GetDetails(string key);
    Task<IReadOnlyList<StorePathDetail>> Search(string pattern);

    Task<Option> BreakLease(string key);
    Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist);
    Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration);
    Task<Option> Release(string leaseId);
}

//public interface IKeyReadWriteStore
//{
//    Task<Option<string>> Append(DataETag data, ScopeContext context);
//    Task<Option> Delete(ScopeContext context);
//    Task<Option<DataETag>> Get(ScopeContext context);
//    Task<Option<string>> Set(DataETag data, ScopeContext context);
//    Task<Option> Release(ScopeContext context);
//}
