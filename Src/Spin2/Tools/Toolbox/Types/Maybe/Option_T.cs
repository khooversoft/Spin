using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Toolbox.Types;

public interface IOption
{
    StatusCode StatusCode { get; }
    bool HasValue { get; }
    object ValueObject { get; }
    string? Error { get; }
}

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
    public Option() => (HasValue, Value, StatusCode) = (false, default!, default);

    [SetsRequiredMembers()]
    public Option(bool hasValue, T value) => (HasValue, Value, StatusCode) = (hasValue, value, hasValue ? StatusCode.OK : StatusCode.NoContent);

    [SetsRequiredMembers()]
    public Option(bool hasValue, T value, StatusCode statusCode) => (HasValue, Value, StatusCode) = (hasValue, value, statusCode);

    [SetsRequiredMembers()]
    public Option(bool hasValue, T value, StatusCode statusCode, string? error) => (HasValue, Value, StatusCode, Error) = (hasValue, value, statusCode, error);

    [SetsRequiredMembers()]
    public Option(StatusCode statusCode) => (StatusCode, Value) = (statusCode, default!);

    [SetsRequiredMembers()]
    public Option(StatusCode statusCode, string? error) => (StatusCode, Value, Error) = (statusCode, default!, error);

    [SetsRequiredMembers()]
    public Option(T value, StatusCode statusCode) : this(value) => StatusCode = statusCode;

    [SetsRequiredMembers()]
    [JsonConstructor]
    public Option(T value, StatusCode statusCode, string? error) : this(value) => (StatusCode, Error) = (statusCode, error);

    [SetsRequiredMembers()]
    public Option(T? value)
    {
        (HasValue, StatusCode, Value) = value switch
        {
            null => (false, StatusCode.NoContent, value!),
            _ => (true, StatusCode.OK, value!),
        };
    }

    public StatusCode StatusCode { get; }
    public bool HasValue { get; }
    public T Value { get; }
    public string? Error { get; }
    object IOption.ValueObject => Value!;

    public override bool Equals(object? obj) =>
        obj is Option<T> maybe &&
        HasValue == maybe.HasValue &&
        StatusCode == maybe.StatusCode &&
        Error == maybe.Error &&
        EqualityComparer<T>.Default.Equals(Value, maybe.Value);

    public bool Equals(Option<T> obj) =>
        obj is Option<T> maybe &&
        HasValue == maybe.HasValue &&
        StatusCode == maybe.StatusCode &&
        Error == maybe.Error &&
        EqualityComparer<T>.Default.Equals(Value, maybe.Value);

    public override int GetHashCode() => HashCode.Combine(Value);

    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
    public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);
    public static implicit operator Option<T>(T value) => new Option<T>(value);
    public static implicit operator Option<T>(StatusCode value) => new Option<T>(value);

    public static Option<T> None { get; } = default;
}
