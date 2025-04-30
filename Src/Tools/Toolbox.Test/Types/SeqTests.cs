using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class SeqTests
{
    [Fact]
    public void Empty()
    {
        var seq = new Seq<int>();
        seq.Count.Be(0);

        Verify.Throw<ArgumentException>(() =>
        {
            seq.First();
        });

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Last();
        });

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Next();
        });

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Back();
        });

        seq.TryGetNext(out int next).BeFalse();
        seq.TryGetPrevious(out int previous).BeFalse();
    }

    [Fact]
    public void SingleForwardBack()
    {
        var seq = new Seq<int>([1]);
        seq.Count.Be(1);
        seq.Next().Be(1);

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Next();
        });

        seq.Back().Be(1);

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Back();
        });

        seq.First().Be(1);
        seq.Last().Be(1);
    }

    [Fact]
    public void SingleBackForward()
    {
        var seq = new Seq<int>([1]);
        seq.Count.Be(1);

        seq.End();
        seq.Back().Be(1);

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Back();
        });

        seq.Next().Be(1);

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Next();
        });
    }

    [Fact]
    public void DoubleForwardBack()
    {
        var seq = new Seq<int>([1, 2]);
        seq.Count.Be(2);
        seq.Next().Be(1);
        seq.Next().Be(2);

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Next();
        });

        seq.Back().Be(2);
        seq.Back().Be(1);

        Verify.Throw<ArgumentException>(() =>
        {
            seq.Back();
        });

        seq.First().Be(1);
        seq.Last().Be(2);
    }

    [Fact]
    public void SingleForwardBackWithTry()
    {
        var seq = new Seq<int>([1]);
        seq.Count.Be(1);

        seq.TryGetNext(out int next1).BeTrue();
        next1.Be(1);

        seq.TryGetNext(out int next).BeFalse();

        seq.TryGetPrevious(out int p1).BeTrue();
        p1.Be(1);

        seq.TryGetNext(out int _).BeFalse();
    }

    [Fact]
    public void SingleBackForwardWithTry()
    {
        var seq = new Seq<int>([1]);
        seq.Count.Be(1);
        seq.End();

        seq.TryGetPrevious(out int p1).BeTrue();
        p1.Be(1);

        seq.TryGetPrevious(out int _).BeFalse();

        seq.TryGetNext(out int next1).BeTrue();
        next1.Be(1);

        seq.TryGetNext(out int next).BeFalse();
    }

    [Fact]
    public void DoubleForwardBackWithTry()
    {
        var seq = new Seq<int>([1, 2]);
        seq.Count.Be(2);

        seq.TryGetNext(out int next1).BeTrue();
        next1.Be(1);

        seq.TryGetNext(out int previous1).BeTrue();
        previous1.Be(2);

        seq.TryGetNext(out int next).BeFalse();

        seq.TryGetPrevious(out int previous2).BeTrue();
        previous2.Be(2);

        seq.TryGetPrevious(out int previous3).BeTrue();
        previous3.Be(1);

        seq.TryGetPrevious(out int _).BeFalse();
    }
}
