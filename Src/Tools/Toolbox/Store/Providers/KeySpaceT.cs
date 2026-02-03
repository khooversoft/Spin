using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class KeySpace<T> : IKeyStore<T>
{
    private readonly KeySpace _keySpace;
    public KeySpace(KeySpace keySpace) => _keySpace = keySpace.NotNull();

    public Task<Option<string>> Add(string key, T data)
    {
        data.NotNull();

        DataETag dataEtag = data.ToJson().ToDataETag();
        return _keySpace.Add(key, dataEtag);
    }

    public Task<Option<string>> Append(string key, T data, string? leaseId = null)
    {
        data.NotNull();

        DataETag dataEtag = data.ToJson().ToDataETag();
        return _keySpace.Append(key, dataEtag, leaseId);
    }

    public Task<Option> Delete(string key, string? leaseId = null) => _keySpace.Delete(key, leaseId);

    public Task<Option> DeleteFolder(string key) => _keySpace.DeleteFolder(key);

    public Task<Option> Exists(string key) => _keySpace.Exists(key);

    public async Task<Option<T>> Get(string key)
    {
        Option<DataETag> getOption = await _keySpace.Get(key);
        if (getOption.IsError()) return getOption.ToOptionStatus<T>();

        var value = getOption.Return().ToObject<T>();
        return value;
    }

    public Task<Option<StorePathDetail>> GetDetails(string key) => _keySpace.GetDetails(key);

    public Task<IReadOnlyList<StorePathDetail>> Search(string pattern, int index = 0, int size = -1) => _keySpace.Search(pattern, index, size);

    public Task<Option<string>> Set(string key, T data, string? leaseId = null)
    {
        DataETag dataEtag = data.ToJson().ToDataETag();
        return _keySpace.Set(key, dataEtag, leaseId);
    }

    public Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist) => _keySpace.AcquireExclusiveLock(key, breakLeaseIfExist);

    public Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration) => _keySpace.AcquireLease(key, leaseDuration);

    public Task<Option> BreakLease(string key) => _keySpace.BreakLease(key);

    public Task<Option> ReleaseLease(string key, string leaseId) => _keySpace.ReleaseLease(key, leaseId);

    public Task<Option> RenewLease(string key, string leaseId) => _keySpace.RenewLease(key, leaseId);
}
