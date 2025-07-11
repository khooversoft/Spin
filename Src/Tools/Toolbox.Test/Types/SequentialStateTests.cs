using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class SequentialStateTests
{
    [Fact]
    public void SequentialStateFlow()
    {
        var state = new SequentialState();
        state.State.Be(0);
        state.MoveState(0).BeFalse();
        state.State.Be(0);
        state.MoveState(1).BeTrue();
        state.State.Be(1);
        state.MoveState(1).BeFalse();
        state.State.Be(1);
        state.MoveState(2).BeTrue();
        state.State.Be(2);
        state.MoveState(2).BeFalse();
        state.State.Be(2);
    }

    [Fact]
    public void SequentialStateFlowWithReset()
    {
        var state = new SequentialState();
        state.State.Be(0);
        state.MoveState(1).BeTrue();
        state.State.Be(1);
        state.MoveState(2).BeTrue();
        state.State.Be(2);
        state.Reset();
        state.State.Be(0);
        state.MoveState(3).BeFalse();
        state.State.Be(0);
        state.MoveState(1).BeTrue();
        state.State.Be(1);
    }
}
