using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public class Sequence<T> : List<T>
{
    public Sequence() { }
    public Sequence(IEnumerable<T> values) : base(values.ToList()) { }

    public new Sequence<T> Add(T value) => this.Action(_ => base.Add(value));
    //public Sequence<T> AddRange(IEnumerable<T> value) => this.Action(_ => base.AddRange(value));

    public override bool Equals(object? obj)
    {
        return obj is Sequence<T> sequence &&
               Count == sequence.Count &&
               this.SequenceEqual(sequence);
    }

    public override int GetHashCode() => base.GetHashCode();

    public static Sequence<T> operator +(Sequence<T> sequence, T value) => sequence.Action(x => x.Add(value));
    public static Sequence<T> operator +(Sequence<T> sequence, IEnumerable<T> values) => sequence.Action(x => x.AddRange(values));
    public static Sequence<T> operator +(Sequence<T> sequence, T[] values) => sequence.Action(x => x.AddRange(values));

    public static bool operator ==(Sequence<T>? left, Sequence<T>? right) => EqualityComparer<Sequence<T>>.Default.Equals(left, right);
    public static bool operator !=(Sequence<T>? left, Sequence<T>? right) => !(left == right);

    public static implicit operator Sequence<T>(T[] values) => new Sequence<T>(values);
}


public static class Sequence
{
    public static Sequence<T> ToSequence<T>(this IEnumerable<T> values) => new(values.NotNull());
}