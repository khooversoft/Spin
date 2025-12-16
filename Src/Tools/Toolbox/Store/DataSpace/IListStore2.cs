using Toolbox.Types;

namespace Toolbox.Store;

public interface IListStore2<T>
{
    Task<Option<string>> Append(string key, IEnumerable<T> data, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> Get(string key, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> Get(string key, string pattern, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex, ScopeContext context);
    Task<IReadOnlyList<StorePathDetail>> Search(string key, string pattern, ScopeContext context);
}
