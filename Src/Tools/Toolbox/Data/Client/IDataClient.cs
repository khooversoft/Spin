using Toolbox.Types;

namespace Toolbox.Data;

public interface IDataClient
{
    public Task<Option> Delete(string key, ScopeContext context);
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get<T>(string key, ScopeContext context) => Get<T>(key, null, context);
    public Task<Option<T>> Get<T>(string key, object? state, ScopeContext context);
    public Task<Option> Set<T>(string key, T value, ScopeContext context) => Set<T>(key, value, null, context);
    public Task<Option> Set<T>(string key, T value, object? state, ScopeContext context);
}

public interface IDataClient<T>
{
    public Task<Option> Delete(string key, ScopeContext context);
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get(string key, ScopeContext context) => Get(key, null, context);
    public Task<Option<T>> Get(string key, object? state, ScopeContext context);
    public Task<Option> Set(string key, T value, ScopeContext context) => Set(key, value, null, context);
    public Task<Option> Set(string key, T value, object? state, ScopeContext context);
}

public interface IDataProvider
{
    DataClientCounters Counters { get; }
    public Task<Option> Delete(string key, ScopeContext context);
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get<T>(string key, object? state, ScopeContext context);
    public Task<Option> Set<T>(string key, T value, object? state, ScopeContext context);
}

