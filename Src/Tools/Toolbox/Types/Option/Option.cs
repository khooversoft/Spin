using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Toolbox.Types;


[DebuggerDisplay("StatusCode={StatusCode}, Error={Error}")]
public readonly struct Option : IEquatable<Option>
{
    public static Option Root { get; } = new Option(StatusCode.OK);

    public Option(StatusCode statusCode) => StatusCode = statusCode;

    [JsonConstructor]
    public Option(StatusCode statusCode, string? error) => (StatusCode, Error) = (statusCode, error);

    public StatusCode StatusCode { get; }
    public string? Error { get; }

    public override string ToString() => $"StatusCode={StatusCode}, Error={Error}";

    public override bool Equals(object? obj) => obj is Option option && Equals(option);
    public bool Equals(Option other) => StatusCode == other.StatusCode && Error == other.Error;
    public override int GetHashCode() => HashCode.Combine(StatusCode, Error);

    public static bool operator ==(Option left, Option right) => left.Equals(right);
    public static bool operator !=(Option left, Option right) => !(left == right);

    public static implicit operator Option(StatusCode value) => new Option(value);
    public static implicit operator Option((StatusCode StatusCode, string Error) value) => new Option(value.StatusCode, value.Error);
}


[DebuggerStepThrough]
public static class OptionExtensions
{
    public static Option<T> ToOptionStatus<T>(this Option subject) => new Option<T>(subject.StatusCode, subject.Error);
    public static Option ToOptionStatus(this (StatusCode statusCode, string? error) subject) => new Option(subject.statusCode, subject.error);

    public static bool IsOk(this Option subject) => subject.StatusCode.IsOk();
    public static bool IsOk(this Option subject, out Option result)
    {
        result = subject;
        return subject.IsOk();
    }

    public static bool IsError(this Option subject) => subject.StatusCode.IsError();
    public static bool IsError(this Option subject, out Option result)
    {
        result = subject;
        return subject.IsError();
    }

    public static bool IsNotFound(this Option subject) => subject.StatusCode.IsNotFound();
    public static bool IsConflict(this Option subject) => subject.StatusCode.IsConflict();
    public static bool IsBadRequest(this Option subject) => subject.StatusCode.IsBadRequest();
    public static bool IsNoContent(this Option subject) => subject.StatusCode.IsNoContent();
    public static bool IsUnauthorized(this Option subject) => subject.StatusCode.IsUnauthorized();
    public static bool IsForbidden(this Option subject) => subject.StatusCode.IsForbidden();
    public static bool IsLocked(this Option subject) => subject.StatusCode.IsLocked();
}