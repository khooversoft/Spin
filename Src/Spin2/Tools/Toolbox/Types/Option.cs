using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Types;


[DebuggerDisplay("HasValue={HasValue}, Value={Value}")]
public readonly struct Option<T> : IEquatable<Option<T>>
{
    [SetsRequiredMembers()]
    public Option() : this(false, default!) { }

    [SetsRequiredMembers()]
    public Option(T? value)
    {
        HasValue = value switch
        {
            null => false,
            var v => true,
        };

        Value = value!;
    }

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
        HasValue == maybe.HasValue &&
        EqualityComparer<T>.Default.Equals(Value, maybe.Value);

    public bool Equals(Option<T> obj) =>
        obj is Option<T> maybe &&
        HasValue == maybe.HasValue &&
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
            false => Option<TO>.None
        };
    }

    public static Option<TO> Bind<TO, T>(this Option<T> subject, Func<T, TO> func)
    {
        return subject.HasValue switch
        {
            true => func(subject.Return()).ToOption(),
            false => Option<TO>.None,
        };
    }

    public static T Return<T>(this Option<T> subject) => subject.HasValue switch
    {
        true => subject.Value,
        false => default!,
    };

    public static T Return<T>(this Option<T> subject, Func<T> none) => subject switch
    {
        var v when !v.HasValue => none(),
        var v => v.Return(),
    };


    public static Option<T> ToOption<T>(this T? value) => new Option<T>(value);

    public static Option<T> ToOption<T>(this T value, bool hasValue) => new Option<T>(hasValue, value);

    public static Option<T> ToOption<T>(this (bool hasValue, T value) value) => new Option<T>(value.hasValue, value.value);
}