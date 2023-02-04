using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions.Extensions;
using Toolbox.Types;
using Xunit;

namespace Toolbox.Test.Types;

public class SequenceTests
{
    [Fact]
    public void GivenDuplicateSequence_WhenEqual_ShouldPass()
    {
        var row = new Sequence<string>
        {
            "First",
            "Second",
        };

        var row2 = new Sequence<string>
        {
            "First",
            "Second",
        };

        (row == row2).Should().BeTrue();
    }

    [Fact]
    public void GivenSequence_WhenAdd_ShouldPass()
    {
        var sequence = new Sequence<string>();

        sequence += "first";
        sequence += "second";
        sequence += "third";

        sequence.Count.Should().Be(3);
        sequence[0].Should().Be("first");
        sequence[1].Should().Be("second");
        sequence[2].Should().Be("third");
    }

    [Fact]
    public void GivenSequence_WhenConstructed_ShouldPass()
    {
        var sequence = new Sequence<string>()
            + "first"
            + "second"
            + "third";

        sequence.Count.Should().Be(3);
        sequence[0].Should().Be("first");
        sequence[1].Should().Be("second");
        sequence[2].Should().Be("third");
    }

    [Fact]
    public void GivenSequence_WhenConstructedForJoin_ShouldPass()
    {
        string result = new Sequence<string>()
        {
            "first",
            "second",
            "third"
        }.Join();

        result.Should().Be("firstsecondthird");
    }
}
