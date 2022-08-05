using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Monads;


[DebuggerDisplay("HasValue={HasValue}, Value={_value}")]
public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T? _value = default(T);

    public Option() { }

    public static Option<T> None { get; } = new Option<T>(false, default(T));

    public Option(T value) : this(true, value) { }

    public Option(bool hasValue, T? value)
    {
        _value = value;
        HasValue = hasValue;
    }

    public bool HasValue { get; } = false;
    public T Return() => HasValue ? _value! : throw new ArgumentException("None value");
    public T Return(Func<T> none) => HasValue ? _value! : none.NotNull()();

    public bool TryGetValue(out T? value)
    {
        value = HasValue ? _value : default;
        return HasValue;
    }

    public Option<TO> Bind<TO>(Func<T, Option<TO>> func) => HasValue ? func(_value!) : Option<TO>.None;
    public Option<TO> Bind<TO>(Func<T, TO> func) => HasValue ? func(_value!).Option() : Option<TO>.None;

    public Option<T> Match(Action<T> some, Action none)
    {
        if (HasValue) some(_value!); else none();
        return this;
    }

    public TO Switch<TO>(Func<T, TO> some, Func<TO> none) => HasValue ? some(_value!) : none();

    public override bool Equals(object? obj) => obj is Option<T> maybe && EqualityComparer<T?>.Default.Equals(_value, maybe._value);
    public bool Equals(Option<T> other) => Equals(other);
    public override int GetHashCode() => HashCode.Combine(_value);


    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
    public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

    public static implicit operator T?(Option<T> other) => other._value;
    public static implicit operator Option<T>(T value) => new Option<T>(value);
}
