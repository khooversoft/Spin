﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Types;

/// <summary>
/// Option struct to prevent allocations
/// 
/// Rules:
///     (1) struct (hasValue + value + StatusCode) must == default (i.e. all zeros)
///     (2) HasValue marks if there is a value, null is HasValue == false
/// 
/// hasValue == true when value is provided
/// Value is only valid when hasValue == true
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("HasValue={HasValue}, Value={Value}, StatusCode={StatusCode}")]
public readonly struct Option<T> : IOption, IEquatable<Option<T>>
{
    [SetsRequiredMembers()]
    public Option() : this(false, default!) { }

    [SetsRequiredMembers()]
    public Option(T? value)
    {
        (HasValue, StatusCode) = value switch
        {
            null => (false, StatusCode.NoContent),
            _ => (true, StatusCode.OK),
        };

        Value = value!;
    }

    [SetsRequiredMembers()]
    public Option(StatusCode statusCode)
    {
        StatusCode = statusCode;

        HasValue = false;
        Value = default!;
    }

    [SetsRequiredMembers()]
    public Option(T? value, StatusCode statusCode)
    {
        HasValue = value switch
        {
            null => false,
            _ => true,
        };

        Value = value!;
        StatusCode = statusCode;
    }

    [SetsRequiredMembers()]
    public Option(bool hasValue, StatusCode statusCode, T value)
    {
        HasValue = hasValue;
        Value = value;
        StatusCode = statusCode;
    }

    [SetsRequiredMembers()]
    public Option(bool hasValue, T? value)
    {
        HasValue = hasValue;
        Value = value!;
        StatusCode = hasValue ? StatusCode.OK : StatusCode.NoContent;
    }

    public static Option<T> None { get; } = default;

    public StatusCode StatusCode { get; }

    public bool HasValue { get; }

    public T Value { get; }

    public override bool Equals(object? obj) =>
        obj is Option<T> maybe &&
        HasValue == maybe.HasValue &&
        StatusCode == maybe.StatusCode &&
        EqualityComparer<T>.Default.Equals(Value, maybe.Value);

    public bool Equals(Option<T> obj) =>
        obj is Option<T> maybe &&
        HasValue == maybe.HasValue &&
        StatusCode == maybe.StatusCode &&
        EqualityComparer<T>.Default.Equals(Value, maybe.Value);

    public override int GetHashCode() => HashCode.Combine(Value);

    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
    public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

    public static implicit operator Option<T>(T value) => new Option<T>(value);
}
