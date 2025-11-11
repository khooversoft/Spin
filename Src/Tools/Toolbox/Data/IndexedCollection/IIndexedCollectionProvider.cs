namespace Toolbox.Data;

public interface IIndexedCollectionProvider<TValue>
{
    void Clear();
    void Set(TValue item);
    void Remove(TValue item);
}
