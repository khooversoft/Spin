using Toolbox.Types;

namespace Toolbox.Store;

//public readonly record struct ListPathData
//{
//    public IStorePathDetail PathDetail { get; init; }
//    public IReadOnlyList<DataETag> Data { get; init; }
//}


public interface IListStore<T>
{
    Task<Option<string>> Append(string key, IEnumerable<T> data, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> Get(string key, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> Get(string key, string pattern, ScopeContext context);
    Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex, ScopeContext context);
    Task<IReadOnlyList<IStorePathDetail>> Search(string key, string pattern, ScopeContext context);
}

public interface IListStoreProvider<T> : IListStore<T>
{
    IListStore<T>? InnerHandler { get; set; }
}
