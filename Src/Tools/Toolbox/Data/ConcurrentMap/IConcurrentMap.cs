namespace Toolbox.Data;

public interface IConcurrentMap<TValue>
{
    void Clear();
    void Set(TValue item);
    void Remove(TValue item);
}
