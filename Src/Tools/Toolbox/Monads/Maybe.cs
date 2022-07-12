using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Monads;


[DebuggerDisplay("HasValue={HasValue}, Value={_value}")]
public readonly struct Maybe<T>
{
    private readonly T? _value = default(Maybe<T>);

    public Maybe() { }

    public static Maybe<T> None { get; } = default(Maybe<T>);

    public Maybe(T? value) : this(true, value) { }

    public Maybe(bool hasValue, T? value)
    {
        _value = value;
        HasValue = hasValue;
    }

    public bool HasValue { get; } = false;
    public T? Return() => HasValue ? _value : default(T);

    public Maybe<TO> Bind<TO>(Func<T?, Maybe<TO>> func) => HasValue ? func(_value) : Maybe<TO>.None;

    public override bool Equals(object? obj) => obj is Maybe<T> maybe && EqualityComparer<T?>.Default.Equals(_value, maybe._value);
    public override int GetHashCode() => HashCode.Combine(_value);
    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);
    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);

    public static implicit operator T?(Maybe<T> other) => other._value;
    public static implicit operator Maybe<T>(T? value) => new Maybe<T>(value);
}
