using Toolbox.Types;

namespace Toolbox.Store;

public readonly record struct ListPathData
{
    public IStorePathDetail PathDetail { get; init; }
    public IReadOnlyList<DataETag> Data { get; init; }
}


public interface IListStore
{
    Task<Option<string>> Append(string key, string listType, IEnumerable<DataETag> data, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option<IReadOnlyList<ListPathData>>> Get(string key, ScopeContext context);
    Task<Option<IReadOnlyList<ListPathData>>> Get(string key, string pattern, ScopeContext context);
    Task<Option<IReadOnlyList<ListPathData>>> GetHistory(string key, DateTime timeIndex, ScopeContext context);
    Task<Option<IReadOnlyList<DataETag>>> GetPartition(string key, string listType, DateTime timeIndex, ScopeContext context);
    Task<IReadOnlyList<IStorePathDetail>> Search(string key, string pattern, ScopeContext context);
}
