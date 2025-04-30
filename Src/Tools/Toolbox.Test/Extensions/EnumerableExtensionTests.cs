using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Extensions;

public class EnumerableExtensionTests
{
    [Fact]
    public void SinglePartition()
    {
        var subject = Enumerable.Range(1, 10);
        var result = subject.Partition(10);
        result.Count.Be(1);
        result.First().Count.Be(10);
    }

    [Fact]
    public void TwoEvenPartition()
    {
        var subject = Enumerable.Range(1, 20);
        var result = subject.Partition(10);
        result.Count.Be(2);
        result.First().Count.Be(10);
        result.Skip(1).First().Count.Be(10);
    }

    [Fact]
    public void TwoOddPartition()
    {
        var subject = Enumerable.Range(0, 22);
        var result = subject.Partition(10);
        result.Count.Be(3);
        result.Select(x => x.Count).BeEquivalent([10, 10, 2]);

        var a1 = Enumerable.Range(0, 10).ToArray();
        var a2 = Enumerable.Range(10, 10).ToArray();
        var a3 = Enumerable.Range(20, 2).ToArray();

        result.Zip([a1, a2, a3], (o, i) => (o, i))
            .Select(x => (x.o, x.i, x.o.SequenceEqual(x.i)))
            .All(x => x.o.SequenceEqual(x.i))
            .BeTrue();
    }
}
