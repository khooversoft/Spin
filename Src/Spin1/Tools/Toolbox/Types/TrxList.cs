using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public sealed record TrxList<T> : IEnumerable<T> where T : class
{
    public int Count => Committed.Count + Items.Count;
    public IReadOnlyList<T> Committed { get; init; } = Array.Empty<T>();
    public IList<T> Items { get; init; } = new List<T>();

    public TrxList<T> Add(T value) => this.Action(_ => Items.Add(value));

    public bool Equals(TrxList<T>? obj) => obj is TrxList<T> list &&
        Committed.SequenceEqual(list.Committed) &&
        Items.SequenceEqual(list.Items);

    public override int GetHashCode() => HashCode.Combine(Committed, Items);

    public IEnumerator<T> GetEnumerator() => Committed.Concat(Items).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


public static class TrxListExtensions
{
    public static bool IsValid<T>(this TrxList<T> subject) where T : class =>
        subject != null &&
        subject.Committed != null &&
        subject.Items != null;
}
