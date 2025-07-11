using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

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

        (row == row2).BeTrue();
    }

    [Fact]
    public void GivenSequence_WhenAdd_ShouldPass()
    {
        var sequence = new Sequence<string>();

        sequence += "first";
        sequence += "second";
        sequence += "third";

        sequence.Count.Be(3);
        sequence[0].Be("first");
        sequence[1].Be("second");
        sequence[2].Be("third");
    }

    [Fact]
    public void GivenSequence_WhenConstructed_ShouldPass()
    {
        var sequence = new Sequence<string>()
            + "first"
            + "second"
            + "third";

        sequence.Count.Be(3);
        sequence[0].Be("first");
        sequence[1].Be("second");
        sequence[2].Be("third");
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

        result.Be("firstsecondthird");
    }
}
