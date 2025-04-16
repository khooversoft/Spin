using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed class GraphMapCounter
{
    public NodeCounter Nodes { get; } = new NodeCounter();
    public EdgeCounter Edges { get; } = new EdgeCounter();
    public LeaseCounter Leases { get; } = new LeaseCounter();
}

public sealed class NodeCounter : CounterBase
{
    internal CounterMeasure ForeignKeyAdded { get; } = new();
    internal CounterMeasure ForeignKeyRemoved { get; } = new();
}

public sealed class EdgeCounter : CounterBase
{
}

public abstract class CounterBase
{
    internal CounterMeasure Added = new();
    internal CounterMeasure Deleted = new();
    internal CounterMeasure Updated = new();
    internal CounterMeasure IndexHit = new();
    internal CounterMeasure IndexMissed = new();
    internal CounterMeasure IndexScan = new();
    internal GaugeMeasure Count = new();

    internal void Index(bool test) => test.IfElse(() => IndexHit.Add(), () => IndexMissed.Add());
    internal void Index(Option option) => option.IsOk().IfElse(() => IndexHit.Add(), () => IndexMissed.Add());
    internal void Index<T>(Option<T> option) => option.IsOk().IfElse(() => IndexHit.Add(), () => IndexMissed.Add());
}

public sealed class LeaseCounter
{
    internal GaugeMeasure ActiveAcquire = new();
    internal GaugeMeasure ActiveExclusive = new();
    internal CounterMeasure Acquire = new();
    internal CounterMeasure Release = new();
}

public sealed class CounterMeasure
{
    private long _value;
    public long Value => _value;
    public void Add(long value = 1) => Interlocked.Add(ref _value, value);
    public void Add(bool test) => Interlocked.Add(ref _value, test ? 1 : 0);
    public void Add(Option option) => Interlocked.Add(ref _value, option.IsOk() ? 1 : 0);

    public static CounterMeasure operator +(CounterMeasure left, long value)
    {
        left.Add(value);
        return left;
    }

    public static implicit operator long(CounterMeasure subject) => subject.Value;
}

public sealed class GaugeMeasure
{
    private long _value;
    public long Value => _value;
    public void Record(long value) => Interlocked.Exchange(ref _value, value);

    public static implicit operator long(GaugeMeasure subject) => subject.Value;
}
