using Toolbox.Types;

namespace Toolbox.Store;

public interface IListStore<T>
{
    Task<Option<string>> Append(string key, IEnumerable<T> data);
    Task<Option> Delete(string key);
    Task<Option<IReadOnlyList<T>>> Get(string key);
    Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex);
    Task<IReadOnlyList<StorePathDetail>> Search(string key);
}
