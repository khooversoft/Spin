using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IFileStore
{
    Task<Option> Add(string path, DataETag data, ScopeContext context);
    Task<Option> Add<T>(string path, T value, ScopeContext context) where T : class;

    Task<Option> Exist(string path, ScopeContext context);
    Task<Option> Delete(string path, ScopeContext context);

    Task<Option<DataETag>> Get(string path, ScopeContext context);
    Task<Option<T>> Get<T>(string path, ScopeContext context);

    Task<Option> Set(string path, DataETag data, ScopeContext context);
    Task<Option> Set<T>(string path, T value, ScopeContext context) where T : class;
}


public static class IFileStoreExtensions
{
    public static async Task<Option<string>> GetAsString(this IFileStore store, string path, ScopeContext context)
    {
        var getOption = await store.Get(path, context);
        if (getOption.IsError()) return getOption.ToOptionStatus<string>();

        var str = getOption.Value.Data.BytesToString();
        return str;
    }

    public static async Task<Option<T>> Get<T>(this IFileStore store, string path, ScopeContext context)
    {
        var getOption = await store.Get(path, context);
        if (getOption.IsError()) return getOption.ToOptionStatus<T>();

        var value = getOption.Value.Data.BytesToString().ToObject<T>().NotNull();
        return value;
    }
}