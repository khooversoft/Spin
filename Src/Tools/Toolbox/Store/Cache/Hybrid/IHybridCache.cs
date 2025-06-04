using Toolbox.Types;

namespace Toolbox.Store;

public interface IHybridCache
{
    public string Name { get; }
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get<T>(string key, ScopeContext context);
    public Task<Option> Set<T>(string key, T value, ScopeContext context);
    public Task<Option> Delete<T>(string key, ScopeContext context);
}

public interface IHybridCacheProvider : IHybridCache
{
    HybridCacheCounters Counters { get; }
}

