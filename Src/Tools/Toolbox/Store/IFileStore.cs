using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IFileStore
{
    Task<Option<string>> Add(string path, DataETag data, ScopeContext context);
    Task<Option> Exist(string path, ScopeContext context);
    Task<Option> Delete(string path, ScopeContext context);
    Task<IReadOnlyList<string>> Search(string pattern, ScopeContext context);
    Task<Option<DataETag>> Get(string path, ScopeContext context);
    Task<Option<string>> Set(string path, DataETag data, ScopeContext context);
}


public static class IFileStoreExtensions
{
    public static Task<Option<string>> Add<T>(this IFileStore store, string path, T value, ScopeContext context) where T : class
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

    public static async Task<Option<DataETag<T>>> Get<T>(this IFileStore store, string path, ScopeContext context)
    {
        try
        {
            Option<DataETag<T>> option = (await store.Get(path, context)) switch
            {
                var v when v.IsOk() => new DataETag<T>(v.Return().ToObject<T>().NotNull(), v.Return().ETag),
                var v => v.ToOptionStatus<DataETag<T>>(),
            };

            return option;
        }
        catch
        {
            return StatusCode.InternalServerError;
        }
    }

    public static Task<Option<string>> Set<T>(this IFileStore store, string path, T value, ScopeContext context) where T : class
    {
        DataETag data = value.ToDataETag();
        return store.Set(path, data, context);
    }
}