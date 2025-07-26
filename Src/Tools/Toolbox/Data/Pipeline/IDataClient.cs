using Toolbox.Types;

namespace Toolbox.Data;

public interface IDataClient
{
    Task<Option> Append(string key, DataETag data, ScopeContext context);
    Task<Option> AppendList(string key, IEnumerable<DataETag> values, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option> DeleteList(string key, ScopeContext context);
    Task<Option> Drain(ScopeContext context);
    Task<Option<DataETag>> Get(string key, ScopeContext context);
    Task<Option<IReadOnlyList<DataETag>>> GetList(string key, ScopeContext context);
    Task<Option> Set(string key, DataETag value, ScopeContext context);

    Task<Option> ReleaseLock(string key, ScopeContext context);
}



public interface IDataClient<T>
{
    Task<Option> Append(string key, T value, ScopeContext context);
    Task<Option> AppendList(string key, IEnumerable<T> values, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option> DeleteList(string key, ScopeContext context);
    Task<Option> Drain(ScopeContext context);
    Task<Option<T>> Get(string key, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> GetList(string key, ScopeContext context);
    Task<Option> Set(string key, T value, ScopeContext context);

    Task<Option> ReleaseLock(string key, ScopeContext context);
}

