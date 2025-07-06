namespace Toolbox.Types;

public class SequentialState
{
    private int _state;
    public SequentialState() { }
    public SequentialState(int initializeState) => _state = initializeState;

    public int State => _state;

    public void Reset() => Interlocked.Exchange(ref _state, 0);
    public void SetState(int state) => Interlocked.Exchange(ref _state, state);

    public bool MoveState(int newState)
    {
        int currentValue = Interlocked.CompareExchange(ref _state, newState, newState - 1);
        return currentValue == newState - 1;
    }
}

