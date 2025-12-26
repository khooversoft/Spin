using System.Runtime.CompilerServices;

namespace Toolbox.Types;

public class EnumState<T> where T : struct, Enum
{
    private int _state;

    static EnumState()
    {
        if (Unsafe.SizeOf<T>() != sizeof(int))
            throw new NotSupportedException($"{typeof(T)} must have an 4-byte underlying type.");
    }


    public EnumState() { }
    public EnumState(T value) => _state = ToInt(value);

    public T Value => ToEnum(Volatile.Read(ref _state));

    public void Set(T state) => Interlocked.Exchange(ref _state, ToInt(state));

    public bool IfValue(T state) => Volatile.Read(ref _state) == ToInt(state);

    public T Move(T fromState, T state) =>
        ToEnum(Interlocked.CompareExchange(ref _state, ToInt(state), ToInt(fromState)));

    public bool TryMove(T fromState, T state)
    {
        int from = ToInt(fromState);
        return Interlocked.CompareExchange(ref _state, ToInt(state), from) == from;
    }

    private static int ToInt(T value) => Unsafe.As<T, int>(ref value);
    private static T ToEnum(int value) => Unsafe.As<int, T>(ref value);
}
