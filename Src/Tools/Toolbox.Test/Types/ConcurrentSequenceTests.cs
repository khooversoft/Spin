using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ConcurrentSequenceTests
{
    [Fact]
    public void Empty()
    {
        var sequence = new ConcurrentSequence<int>();
        sequence.Count.Be(0);
        sequence.Count().Be(0);
        sequence.IsReadOnly.BeFalse();
    }

    [Fact]
    public void CtorWithValues()
    {
        var seq = new ConcurrentSequence<int>(new[] { 1, 2, 3 });
        seq.Count.Be(3);
        seq.Contains(2).BeTrue();
        seq.Contains(4).BeFalse();
    }

    [Fact]
    public void Add_Contains_Remove()
    {
        var seq = new ConcurrentSequence<int>();
        seq.Add(10);
        seq.Add(20);
        seq.Count.Be(2);

        seq.Contains(10).BeTrue();
        seq.Contains(30).BeFalse();

        seq.Remove(10).BeTrue();
        seq.Remove(30).BeFalse();
        seq.Count.Be(1);
        seq.Contains(20).BeTrue();
    }

    [Fact]
    public void AddRange_And_Clear()
    {
        var seq = new ConcurrentSequence<int>();
        seq.AddRange(new[] { 1, 2, 3, 4 });
        seq.Count.Be(4);

        seq.Clear();
        seq.Count.Be(0);
        seq.Count().Be(0);
    }

    [Fact]
    public void CopyTo_Valid()
    {
        var seq = new ConcurrentSequence<int>(new[] { 1, 2, 3 });
        var arr = new int[6];
        seq.CopyTo(arr, 2);

        arr[0].Be(0);
        arr[1].Be(0);
        arr[2].Be(1);
        arr[3].Be(2);
        arr[4].Be(3);
        arr[5].Be(0);
    }

    [Fact]
    public void CopyTo_Throws_OnInvalidArgs()
    {
        var seq = new ConcurrentSequence<int>(new[] { 1, 2, 3 });

        Verify.Throws<ArgumentNullException>(() => seq.CopyTo(null!, 0));
        Verify.Throws<ArgumentOutOfRangeException>(() => seq.CopyTo(new int[3], -1));
        Verify.Throws<ArgumentException>(() => seq.CopyTo(new int[4], 2)); // not enough room
    }

    [Fact]
    public void Operators_And_Equality()
    {
        var a = new ConcurrentSequence<int>();
        a += 1;
        a += 2;
        a += new[] { 3, 4 };
        a += (IEnumerable<int>)new List<int> { 5, 6 };

        a.Count.Be(6);
        a.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }).BeTrue();

        // Equality and order sensitivity
        var b = new ConcurrentSequence<int>(new[] { 1, 2, 3, 4, 5, 6 });
        (a == b).BeTrue();
        (a != b).BeFalse();

        var c = new ConcurrentSequence<int>(new[] { 6, 5, 4, 3, 2, 1 });
        (a == c).BeFalse();
        (a != c).BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromArray()
    {
        ConcurrentSequence<int> seq = new[] { 7, 8, 9 };
        seq.Count.Be(3);
        seq.SequenceEqual(new[] { 7, 8, 9 }).BeTrue();
    }

    [Fact]
    public async Task Enumerator_ReturnsSnapshot_WhileMutating()
    {
        var seq = new ConcurrentSequence<int>(Enumerable.Range(0, 1_000));
        var enumerated = 0;

        // Take the enumerator BEFORE starting mutation to ensure we get a 1,000-item snapshot
        var enumerator = seq.GetEnumerator();

        // Start a background mutator that adds elements while we iterate
        var mutate = Task.Run(() =>
        {
            for (int i = 0; i < 1_000; i++)
            {
                seq.Add(1_000 + i);
                // minimal yield to interleave without slowing the test
                Thread.Yield();
            }
        });

        // Iterate the snapshot while mutation happens
        while (enumerator.MoveNext())
        {
            enumerated++;
            // encourage interleaving
            if ((enumerated & 63) == 0) Thread.Yield();
        }

        await mutate;

        // The enumeration should reflect the snapshot taken at GetEnumerator()
        enumerated.Be(1_000);
        // But the current count should include the added items
        seq.Count.Be(2_000);
    }

    [Fact]
    public void ConcurrentAdds_ShouldReachExpectedCount()
    {
        var seq = new ConcurrentSequence<int>();
        int total = 20_000;
        // Parallel.For is efficient and deterministic for this use
        Parallel.For(0, total, i => seq.Add(i));
        seq.Count.Be(total);

        // Validate we can enumerate all elements; snapshot enumeration is OK
        seq.Count().Be(total);
    }

    [Fact]
    public void Dispose_IsIdempotent_And_PreventsFurtherUse()
    {
        var seq = new ConcurrentSequence<int>(new[] { 1, 2, 3 });
        seq.Dispose();
        // Second dispose should not throw
        seq.Dispose();

        // Any operation that touches the lock should throw ObjectDisposedException
        Verify.Throws<ObjectDisposedException>(() => { var _ = seq.Count; });
        Verify.Throws<ObjectDisposedException>(() => seq.Add(4));
        Verify.Throws<ObjectDisposedException>(() => { foreach (var _ in seq) { } });
    }
}
