using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools;

namespace Toolbox.Test.Types;

public class SequentialStateTests
{
    [Fact]
    public void SequentialStateFlow()
    {
        var state = new SequentialState();
        state.MoveState(0).Should().BeFalse();
        state.MoveState(1).Should().BeTrue();
        state.MoveState(1).Should().BeFalse();
        state.MoveState(2).Should().BeTrue();
    }

    [Fact]
    public void SequentialStateFlowWithReset()
    {
        var state = new SequentialState();
        state.MoveState(1).Should().BeTrue();
        state.MoveState(2).Should().BeTrue();
        state.Reset();
        state.MoveState(3).Should().BeFalse();
        state.MoveState(1).Should().BeTrue();
    }
}
