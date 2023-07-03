using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Types;

[GenerateSerializer, Immutable]
public record SpinResponse
{
    public SpinResponse(StatusCode statusCode) => StatusCode = statusCode;
    public SpinResponse(StatusCode statusCode, string? error) => (StatusCode, Error) = (statusCode, error);

    [Id(0)] public StatusCode StatusCode { get; }
    [Id(2)] public string? Error { get; }
}

[GenerateSerializer, Immutable]
public record SpinResponse<T>
{
    public SpinResponse(StatusCode statusCode) => StatusCode = statusCode;
    public SpinResponse(StatusCode statusCode, string? error) => (StatusCode, Error) = (statusCode, error);
    public SpinResponse(T? value) => (Value, StatusCode) = value switch
    {
        null => (default, StatusCode.NotFound),
        var v => (v, StatusCode.OK),
    };
    public SpinResponse(T value, StatusCode statusCode) => (Value, StatusCode) = (Value, StatusCode.OK);

    [Id(0)] public StatusCode StatusCode { get; }
    [Id(1)] public T? Value { get; }
    [Id(2)] public string? Error { get; }

    public static implicit operator SpinResponse<T>(T value) => new SpinResponse<T>(value);
}


public static class SpinResponseExtensions
{
    public static SpinResponse ToSpinResponse(this ValidatorResult value) => new SpinResponse(value.IsValid ? StatusCode.OK : StatusCode.BadRequest, value.FormatErrors());
    public static SpinResponse<T> ToSpinResponse<T>(this T value) => new SpinResponse<T>(value);

    public static T Return<T>(this SpinResponse<T> subject) => subject.StatusCode.IsOk() switch
    {
        true => subject.Value.NotNull(),
        false => throw new ArgumentException("Value is null"),
    };

    public static Option<T> ToOption<T>(this SpinResponse<T> subject) => new Option<T>(subject.Value, subject.StatusCode, subject.Error);
    public static StatusResponse ToStatusResponse(this SpinResponse subject) => new StatusResponse(subject.StatusCode, subject.Error);
}