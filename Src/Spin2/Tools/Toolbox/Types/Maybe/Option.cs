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

    public override bool Equals(object? obj) => obj is Option option && Equals(option);
    public bool Equals(Option other) => StatusCode == other.StatusCode && Error == other.Error;
    public override int GetHashCode() => HashCode.Combine(StatusCode, Error);

    public static bool operator ==(Option left, Option right) => left.Equals(right);
    public static bool operator !=(Option left, Option right) => !(left == right);

    public static implicit operator Option(StatusCode value) => new Option(value);
}

public static class OptionExtensions
{
    public static bool IsOk(this Option subject) => subject.StatusCode.IsOk();
    public static bool IsError(this Option subject) => subject.StatusCode.IsError();

    public static Option Test(this Option option, Func<Option> test)
    {
        if (option.IsError()) return option;
        return test();
    }

    public static async Task<Option> TestAsync(this Task<Option> option, Func<Option> test)
    {
        var o = await option;
        if (o.IsError()) return o;

        return test();
    }

    public static async Task<Option> TestAsync(this Task<Option> option, Func<Task<Option>> test)
    {
        var o = await option;
        if (o.IsError()) return o;

        return await test();
    }
}