using Toolbox.Types;

namespace Toolbox.Store;

public interface IHybridCache
{
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get<T>(string key, ScopeContext context);
    public Task<Option> Set<T>(string key, T value, ScopeContext context);
    public Task<Option> Delete(string key, ScopeContext context);
}

public interface IHybridCache<T>
{
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get(string key, ScopeContext context);
    public Task<Option> Set(string key, T value, ScopeContext context);
    public Task<Option> Delete(string key, ScopeContext context);
}

public interface IHybridCacheProvider
{
    public string Name { get; }
    HybridCacheCounters Counters { get; }
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get<T>(string key, ScopeContext context);
    public Task<Option> Set<T>(string key, T value, ScopeContext context);
    public Task<Option> Delete(string key, ScopeContext context);
}

