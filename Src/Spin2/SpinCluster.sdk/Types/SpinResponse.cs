using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Types;

[GenerateSerializer, Immutable]
public record SpinResponse<T>
{
    public SpinResponse() => StatusCode = StatusCode.NotFound;
    public SpinResponse(StatusCode statusCode) => StatusCode = statusCode;
    public SpinResponse(StatusCode statusCode, string? error) => (StatusCode, Error) = (statusCode, error);
    public SpinResponse(T? value) => (Value, StatusCode) = value switch
    {
        null => (default, StatusCode.NotFound),
        var v => (v, StatusCode.OK),
    };
    public SpinResponse(T value, StatusCode statusCode) => (Value, StatusCode) = (Value, StatusCode.OK);

    [Id(0)] public StatusCode StatusCode { get; init; }
    [Id(1)] public T? Value { get; init; }
    [Id(2)] public string? Error { get; init; }

    public static implicit operator SpinResponse<T>(T value) => new SpinResponse<T>(value);

    public static SpinResponse<T> NotFound() => new SpinResponse<T> { StatusCode = StatusCode.NotFound };
}


public static class SpinResponseExtensions
{
    public static SpinResponse<T> ToSpinResponse<T>(this T value) => new SpinResponse<T>(value);

    public static T Return<T>(this SpinResponse<T> subject) => subject.StatusCode.IsOk() switch
    {
        true => subject.Value.NotNull(),
        false => throw new ArgumentException("Value is null"),
    };

    public static Option<T> ToObject<T>(this SpinResponse<T> subject) => new Option<T>(subject.Value, subject.StatusCode, subject.Error);
}