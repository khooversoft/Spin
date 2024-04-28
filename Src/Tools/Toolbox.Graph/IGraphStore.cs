using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphStore
{
    Task<Option<string>> Add(string nodeKey, string name, DataETag data, ScopeContext context);
    Task<Option> Delete(string nodeKey, string name, ScopeContext context);
    Task<Option> Exist(string nodeKey, string name, ScopeContext context);
    Task<Option<DataETag>> Get(string nodeKey, string name, ScopeContext context);
    Task<Option<string>> Set(string nodeKey, string name, DataETag data, ScopeContext context);
}


public static class IGraphStore2Extensions
{
    public static Task<Option<string>> Add<T>(this IGraphStore store, string nodeKey, string name, T value, ScopeContext context) where T : class
    {
        return store.Add(nodeKey, name, value.ToDataETag(), context);
    }

    public static async Task<Option<T>> Get<T>(this IGraphStore store, string nodeKey, string name, ScopeContext context) where T : class
    {
        Option<DataETag> result = await store.Get(nodeKey, name, context);
        if (result.IsError()) return result.ToOptionStatus<T>();

        return result.Return().ToObject<T>();
    }

    public static Task<Option<string>> Set<T>(this IGraphStore store, string nodeKey, string name, T value, ScopeContext context) where T : class
    {
        return store.Set(nodeKey, name, value.ToDataETag(), context);
    }
}