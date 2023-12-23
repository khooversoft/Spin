using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Toolbox.Types;

[DebuggerDisplay("StatusCode={StatusCode}, Error={Error}")]
public readonly struct Option : IEquatable<Option>
{
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

