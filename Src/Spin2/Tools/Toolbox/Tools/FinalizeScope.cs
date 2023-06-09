namespace Toolbox.Tools;

public class FinalizeScope<T> : IDisposable
{
    private readonly T _value;
    private Action<T>? _finalizeAction;

    public FinalizeScope(T value, Action<T> finalizeAction)
    {
        _finalizeAction = finalizeAction.NotNull();
        _value = value;
    }

    public void Dispose() => Interlocked.Exchange(ref _finalizeAction, null)?.Invoke(_value);
}
