using Toolbox.Types;

namespace Toolbox.Data;


public interface IJournalClient<T>
{
    Task<Option> Append(IEnumerable<T> values, ScopeContext context);
    Task<Option> Delete(ScopeContext context);
    Task<Option<IReadOnlyList<T>>> Get(ScopeContext context);
}
