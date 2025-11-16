namespace Toolbox.Types;

public class ReferenceState<T> where T : class
{
    private T? _value;

    public ReferenceState() { }
    public ReferenceState(T initialValue) => _value = initialValue;

    public T? Value => Volatile.Read(ref _value);
    public void Set(T value) => Interlocked.Exchange(ref _value, value);
    public T? Move(T fromState, T state) => Interlocked.CompareExchange(ref _value, state, fromState);
    public bool TryMove(T? fromState, T? state) => Interlocked.CompareExchange(ref _value, state, fromState) switch
    {
        null => fromState == null,
        var v => v.Equals(fromState),
    };
}