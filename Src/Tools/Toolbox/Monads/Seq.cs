using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolbox.Monads;

public static class Seq
{
    public static IEnumerable<Option<T>> Enumerable<T>()
    {
        yield break;
    }
}

public readonly struct Seq<T>
{

    public Seq(Unit<T> value)
    {
        Value = value.Return();
        Values = Seq.Enumerable<T>();
    }

    public Seq(Unit<T> value, IEnumerable<Option<T>> values)
    {
        Value = value.Return();
        Values = values;
    }

    public T Value { get; }
    public IEnumerable<Option<T>> Values { get; }

    public Seq<T> Bind(Func<T, Option<T>> func) => new Seq<T>(Value.Unit(), Values.Append(func(Value)));
    public IEnumerable<Option<T>> Return() => Values;

}


public readonly struct Seq<TRoot, T>
{
    public Seq(Unit<TRoot> value)
    {
        Value = value.Return();
        Values = Seq.Enumerable<T>();
    }

    public Seq(Unit<TRoot> value, IEnumerable<Option<T>> values)
    {
        Value = value.Return();
        Values = values;
    }

    public TRoot Value { get; }
    public IEnumerable<Option<T>> Values { get; }

    public Seq<TRoot, T> Bind(Func<TRoot, Option<T>> func) => new Seq<TRoot, T>(Value.Unit(), Values.Append(func(Value)));
    public IEnumerable<Option<T>> Return() => Values;
}
