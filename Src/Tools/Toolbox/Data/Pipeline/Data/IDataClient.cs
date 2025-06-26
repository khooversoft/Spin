using Toolbox.Types;

namespace Toolbox.Data;

public interface IDataClient<T>
{
    Task<Option> Append(string key, T value, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option<T>> Get(string key, ScopeContext context);
    Task<Option> Set(string key, T value, ScopeContext context);
}

