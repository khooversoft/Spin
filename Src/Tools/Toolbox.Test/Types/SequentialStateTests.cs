using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Types;

public class SequentialStateTests
{
    [Fact]
    public void SequentialStateFlow()
    {
        var state = new SequentialState();
        state.State.Should().Be(0);
        state.MoveState(0).Should().BeFalse();
        state.State.Should().Be(0);
        state.MoveState(1).Should().BeTrue();
        state.State.Should().Be(1);
        state.MoveState(1).Should().BeFalse();
        state.State.Should().Be(1);
        state.MoveState(2).Should().BeTrue();
        state.State.Should().Be(2);
        state.MoveState(2).Should().BeFalse();
        state.State.Should().Be(2);
    }

    [Fact]
    public void SequentialStateFlowWithReset()
    {
        var state = new SequentialState();
        state.State.Should().Be(0);
        state.MoveState(1).Should().BeTrue();
        state.State.Should().Be(1);
        state.MoveState(2).Should().BeTrue();
        state.State.Should().Be(2);
        state.Reset();
        state.State.Should().Be(0);
        state.MoveState(3).Should().BeFalse();
        state.State.Should().Be(0);
        state.MoveState(1).Should().BeTrue();
        state.State.Should().Be(1);
    }
}
