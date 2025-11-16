namespace Toolbox.Types;

public class EnumState<T> where T : struct, Enum
{
    private T _state = default!;

    public EnumState() { }
    public EnumState(T value) => _state = value;

    public T Value => _state;
    public void Set(T state) => Interlocked.Exchange(ref _state, state);
    public T Move(T fromState, T state) => Interlocked.CompareExchange(ref _state, state, fromState);
    public bool TryMove(T fromState, T state) => Interlocked.CompareExchange(ref _state, state, fromState).Equals(fromState);
}
