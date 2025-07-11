namespace Toolbox.Tools;

public record struct FinalizeScope : IDisposable
{
    private Action? _finalizeAction;
    private Action? _cancelAction;

    public FinalizeScope(Action finalizeAction)
    {
        _finalizeAction = finalizeAction.NotNull();
    }

    public FinalizeScope(Action finalizeAction, Action cancelAction)
    {
        _finalizeAction = finalizeAction.NotNull();
        _cancelAction = cancelAction.NotNull();
    }

    public void Cancel()
    {
        _finalizeAction = null;
        Interlocked.Exchange(ref _cancelAction, null)?.Invoke();
    }

    public void Dispose() => Interlocked.Exchange(ref _finalizeAction, null)?.Invoke();
}

public record struct FinalizeScope<T> : IDisposable
{
    private readonly T _value;
    private Action<T>? _finalizeAction;
    private Action<T>? _cancelAction;

    public FinalizeScope(T value, Action<T> finalizeAction)
    {
        _value = value;
        _finalizeAction = finalizeAction.NotNull();
    }

    public FinalizeScope(T value, Action<T> finalizeAction, Action<T> cancelAction)
    {
        _value = value;
        _finalizeAction = finalizeAction.NotNull();
        _cancelAction = cancelAction.NotNull();
    }

    public void Cancel()
    {
        _finalizeAction = null;
        Interlocked.Exchange(ref _cancelAction, null)?.Invoke(_value);
    }

    public void Dispose() => Interlocked.Exchange(ref _finalizeAction, null)?.Invoke(_value);

    public static implicit operator T(FinalizeScope<T> scope) => scope._value;
}
