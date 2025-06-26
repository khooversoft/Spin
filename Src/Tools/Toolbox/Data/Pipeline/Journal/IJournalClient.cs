using Toolbox.Types;

namespace Toolbox.Data;


public interface IJournalClient<T>
{
    Task<Option> Append(string key, IEnumerable<T> values, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> Get(string key, ScopeContext context);
}
