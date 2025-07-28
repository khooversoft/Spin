using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Data;

public class LogSequenceNumberTests
{
    [Fact]
    public void NumberSequence()
    {
        var sn = new LogSequenceNumber();

        var collection = Enumerable.Range(0, 100).Select(x => sn.Next()).ToArray();
        collection.NotNull();
        collection.Length.Be(100);
    }

    [Fact]
    public void NumberSequenceOrder()
    {
        var sn = new LogSequenceNumber();

        var collection = Enumerable.Range(0, 100).Select(x => sn.Next()).ToArray();

        var shuffle = collection.Shuffle();
        collection.SequenceEqual(shuffle).BeFalse();

        var sorted = shuffle.OrderBy(x => x).ToArray();
        collection.SequenceEqual(sorted).BeTrue();
    }
}
