using Toolbox.Extensions;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Types;

public class SequenceJoinTests
{
    [Fact]
    public void GivenSingleSequence_WhenJoined_ShouldPass()
    {
        var list = Enumerable.Range(0, 10)
            .SequenceJoin(99)
            .ToList();

        list.Count.Should().Be(19);

        list
            .Where((x, i) => i % 2 == 0 ? x == i / 2 : x == 99)
            .Count().Should().Be(19);
    }

    [Fact]
    public void GivenSequence_WhenJoinedWithSelect_ShouldPass()
    {
        var list = Enumerable.Range(0, 10)
            .Select(x => $"Label_{x}")
            .SequenceJoin(x => x + ",")
            .ToList();

        list.Count.Should().Be(10);

        var expected = Enumerable.Range(0, 10)
            .Select(x => $"Label_{x}")
            .Select((x, i) => i < 9 ? x + "," : x)
            .Zip(list)
            .ToList();

        expected
            .All(x => x.First == x.Second).Should().BeTrue();
    }
}
