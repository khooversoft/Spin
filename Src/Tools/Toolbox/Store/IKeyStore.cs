using Toolbox.Tools;
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
    Task<IReadOnlyList<StorePathDetail>> Search(string pattern, int index = 0, int size = -1);

    Task<Option> BreakLease(string key);
    Task<Option<string>> AcquireExclusiveLock(string key, bool breakLeaseIfExist);
    Task<Option<string>> AcquireLease(string key, TimeSpan leaseDuration);
    Task<Option> ReleaseLease(string key, string leaseId);
    Task<Option> RenewLease(string key, string leaseId);
}


public static class KeyStoreExtensions
{
    public static async Task<Option<string>> Add<T>(this IKeyStore keyStore, string key, T value)
    {
        var data = value.ToDataETag();
        return await keyStore.NotNull().Add(key, data);
    }

    public static async Task<Option<string>> Set<T>(this IKeyStore keyStore, string key, T value)
    {
        var data = value.ToDataETag();
        return await keyStore.NotNull().Set(key, data);
    }

    public static Task<Option<T>> Get<T>(this IKeyStore keyStore, string key) => Get<T>(keyStore, key, data => data.ToObject<T>());

    public static async Task<Option<T>> Get<T>(this IKeyStore keyStore, string key, Func<DataETag, Option<T>> converter)
    {
        keyStore.NotNull();
        converter.NotNull();

        var getOption = await keyStore.Get(key);
        if (getOption.IsError()) return getOption.ToOptionStatus<T>();

        DataETag data = getOption.Return();
        return converter(data);
    }

}