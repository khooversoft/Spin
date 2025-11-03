namespace Toolbox.Data;

public interface ITrxContext<TValue>
{
    void Add(TValue newValue);
    void Delete(TValue currentValue, TValue newValue);
    void Update(TValue currentValue, TValue newValue);
}
