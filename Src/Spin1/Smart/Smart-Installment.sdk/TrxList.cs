using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Smart_Installment.sdk;

public sealed record TrxList<T> : IEnumerable<T> where T : class
{
    public int Count => Committed.Count + Items.Count;
    public IReadOnlyList<T> Committed { get; init; } = Array.Empty<T>();
    public IList<T> Items { get; init; } = new List<T>();

    public TrxList<T> Add(T value) => this.Action(_ => Items.Add(value));

    public bool Equals(TrxList<T>? obj) => obj is TrxList<T> list &&
        Enumerable.SequenceEqual(Committed, list.Committed) &&
        Enumerable.SequenceEqual(Items, list.Items);

    public override int GetHashCode() => HashCode.Combine(Committed, Items);

    public IEnumerator<T> GetEnumerator() => Committed.Concat(Items).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


public static class TrxListExtensions
{
    public static TrxList<T> Verify<T>(this TrxList<T> subject) where T : class
    {
        subject.NotNull();
        subject.Committed.NotNull();
        subject.Items.NotNull();

        return subject;
    }
}
