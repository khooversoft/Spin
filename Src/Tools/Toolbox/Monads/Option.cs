using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Monads;


[DebuggerDisplay("HasValue={HasValue}, Value={_value}")]
public readonly struct Option<T> : IEquatable<Option<T>>
{
    [SetsRequiredMembers()]
    public Option(T value) : this(true, value) { }

    [SetsRequiredMembers()]
    public Option(bool hasValue, T value)
    {
        Value = value;
        HasValue = hasValue;
    }

    public static Option<T> None { get; } = default;

    public bool HasValue { get; } = false;

    public T Value { get; }

    public override bool Equals(object? obj) =>
        obj is Option<T> maybe &&
        EqualityComparer<T>.Default.Equals(Value, maybe.Value);

    public bool Equals(Option<T> obj) =>
        obj is Option<T> maybe &&
        EqualityComparer<T>.Default.Equals(Value, maybe.Value);

    public override int GetHashCode() => HashCode.Combine(Value);


    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
    public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

    public static implicit operator T(Option<T> other) => other.Value;
    public static implicit operator Option<T>(T value) => new Option<T>(value);
}


public static class OptionExtensions
{
    public static Option<TO> Bind<TO, T>(this Option<T> subject, Func<T, Option<TO>> func)
    {
        return subject.HasValue switch
        {
            true => func(subject.Return()),
            false => Option<TO>.None,
        };
    }

    public static Option<TO> Bind<TO, T>(this Option<T> subject, Func<T, TO> func)
    {
        return subject.HasValue switch
        {
            true => func(subject.Return()).Option(),
            false => Option<TO>.None,
        };
    }

    public static T Return<T>(this Option<T> subject) => subject.Value;

    public static T Return<T>(this Option<T> subject, Func<T> none) => subject switch
    {
        var v when !v.HasValue => none(),
        var v => v.Return(),
    };
}