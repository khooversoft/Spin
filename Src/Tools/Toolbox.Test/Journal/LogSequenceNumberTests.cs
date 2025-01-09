using FluentAssertions;
using Toolbox.Journal;

namespace Toolbox.Test.Journal;

public class LogSequenceNumberTests
{
    [Fact]
    public void NumberSequence()
    {
        var sn = new LogSequenceNumber();

        var collection = Enumerable.Range(0, 100).Select(x => sn.Next()).ToArray();
        collection.Should().NotBeNullOrEmpty();
        collection.Length.Should().Be(100);
    }
}
