namespace Toolbox.Tools;

public class SequentialState
{
    private int _state;
    public SequentialState() { }
    public SequentialState(int initializeState) => _state = initializeState;

    public int State => _state;

    public void Reset() => _state = 0;

    public bool MoveState(int newState)
    {
        int currentValue = Interlocked.CompareExchange(ref _state, newState, newState - 1);
        return currentValue == (newState - 1);
    }
}
