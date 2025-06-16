using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Data;

public abstract class DataProviderBase : IDataProvider
{
    public DataClientCounters Counters => new();
    public virtual Task<Option> Delete(string key, ScopeContext context) => Task.FromResult<Option>(StatusCode.OK);
    public virtual Task<Option<string>> Exists(string key, ScopeContext context) => new Option<string>(StatusCode.NotFound).ToTaskResult();
    public virtual Task<Option<T>> Get<T>(string key, object? state, ScopeContext context) => new Option<T>(StatusCode.NotFound).ToTaskResult();
    public virtual Task<Option> Set<T>(string key, T value, object? state, ScopeContext context) => Task.FromResult<Option>(StatusCode.Conflict);
}
