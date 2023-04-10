using System;
using System.Collections.Generic;

namespace Toolbox.Monads;

public readonly struct Unit<T>
{
    public Unit(T value) => Value = value;

    public T Value { get; }

    public T Return() => Value;

    public override bool Equals(object? obj) => obj is Unit<T> unit &&
               EqualityComparer<T>.Default.Equals(Value, unit.Value);

    public override int GetHashCode() => HashCode.Combine(Value);
    public static bool operator ==(Unit<T> left, Unit<T> right) => left.Equals(right);
    public static bool operator !=(Unit<T> left, Unit<T> right) => !(left == right);
    public static implicit operator Unit<T>(T value) => new Unit<T>(value);
    public static implicit operator T(Unit<T> value) => value.Value;
}
