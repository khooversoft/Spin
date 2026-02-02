using Toolbox.Types;

namespace Toolbox.Store;

public interface ISequenceStore<T>
{
    Task<Option<string>> Add(string key, T data);
    Task<Option> Delete(string key);
    Task<Option<IReadOnlyList<T>>> Get(string key);
    Task<Option<IReadOnlyList<T>>> GetHistory(string key, DateTime timeIndex);
}
