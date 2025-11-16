namespace Toolbox.Types;

public class BoolState
{
    private bool _state;

    public bool Value => Volatile.Read(ref _state);
    public void Reset(bool state) => Interlocked.Exchange(ref _state, state);
    public bool TrySet() => Interlocked.Exchange(ref _state, true) == false;
}
