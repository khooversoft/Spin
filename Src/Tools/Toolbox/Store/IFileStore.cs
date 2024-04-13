using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IFileStore
{
    Task<Option> Add(string path, DataETag data, ScopeContext context);
    Task<Option> Exist(string path, ScopeContext context);
    Task<Option> Delete(string path, ScopeContext context);
    Task<Option<DataETag>> Get(string path, ScopeContext context);
    Task<Option> Set(string path, DataETag data, ScopeContext context);
}


public static class IFileStoreExtensions
{
    public static Task<Option> Add<T>(this IFileStore store, string path, T value, ScopeContext context) where T : class
    {
        string json = value.ToJson();
        DataETag data = new DataETag(json.ToBytes());
        return store.Add(path, data, context);
    }

    public static async Task<Option<string>> GetAsString(this IFileStore store, string path, ScopeContext context)
    {
        var getOption = await store.Get(path, context);
        if (getOption.IsError()) return getOption.ToOptionStatus<string>();

        var str = getOption.Return().Data.BytesToString();
        return str;
    }

    public static async Task<Option<T>> Get<T>(this IFileStore store, string path, ScopeContext context)
    {
        Option<T> option = (await store.Get(path, context)) switch
        {
            var v when v.IsOk() => v.Return().Data.AsSpan().ToObject<T>().NotNull(),
            var v => v.ToOptionStatus<T>(),
        };

        return option;
    }

    public static Task<Option> Set<T>(this IFileStore store, string path, T value, ScopeContext context) where T : class
    {
        DataETag data = value.ToDataETag();
        return store.Set(path, data, context);
    }
}