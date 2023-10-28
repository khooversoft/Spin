namespace Toolbox.Tools;

public record struct FinalizeScope<T> : IDisposable
{
    private readonly T _value;
    private Action<T>? _finalizeAction;

    public FinalizeScope(T value, Action<T> finalizeAction)
    {
        _finalizeAction = finalizeAction.NotNull();
        _value = value;
    }

    public void Cancel() => _finalizeAction = null;

    public void Dispose() => Interlocked.Exchange(ref _finalizeAction, null)?.Invoke(_value);

    public static implicit operator T(FinalizeScope<T> scope) => scope._value;
}
