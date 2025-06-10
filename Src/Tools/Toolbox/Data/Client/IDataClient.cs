using Toolbox.Types;

namespace Toolbox.Store;

public interface IDataClient
{
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get<T>(string key, ScopeContext context);
    public Task<Option> Set<T>(string key, T value, ScopeContext context);
    public Task<Option> Delete(string key, ScopeContext context);
}

public interface IDataClient<T>
{
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get(string key, ScopeContext context);
    public Task<Option> Set(string key, T value, ScopeContext context);
    public Task<Option> Delete(string key, ScopeContext context);
}

public interface IDataProvider
{
    public string Name { get; }
    DataClientCounters Counters { get; }
    public Task<Option<string>> Exists(string key, ScopeContext context);
    public Task<Option<T>> Get<T>(string key, ScopeContext context);
    public Task<Option> Set<T>(string key, T value, ScopeContext context);
    public Task<Option> Delete(string key, ScopeContext context);
}

