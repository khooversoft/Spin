using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Journal;

public class LogSequenceNumberTests
{
    [Fact]
    public void NumberSequence()
    {
        var sn = new LogSequenceNumber();

        var collection = Enumerable.Range(0, 100).Select(x => sn.Next()).ToArray();
        collection.NotNull();
        collection.Length.Should().Be(100);
    }

    [Fact]
    public void NumberSequenceOrder()
    {
        var sn = new LogSequenceNumber();

        var collection = Enumerable.Range(0, 100).Select(x => sn.Next()).ToArray();

        var shuffle = collection.Shuffle();
        Enumerable.SequenceEqual(collection, shuffle).Should().BeFalse();

        var sorted = shuffle.OrderBy(x => x).ToArray();
        Enumerable.SequenceEqual(collection, sorted).Should().BeTrue();
    }
}
