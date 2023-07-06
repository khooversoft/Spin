using Toolbox.Types;

namespace SpinCluster.sdk.Types;

[GenerateSerializer, Immutable]
public record SpinResponse : ISpinResponse
{
    public SpinResponse() { }
    public SpinResponse(StatusCode statusCode) => StatusCode = statusCode;
    public SpinResponse(StatusCode statusCode, string? error) => (StatusCode, Error) = (statusCode, error);

    [Id(0)] public StatusCode StatusCode { get; }
    [Id(2)] public string? Error { get; }
}

[GenerateSerializer, Immutable]
public record SpinResponse<T> : ISpinResponseWithValue
{
    public SpinResponse() { }
    public SpinResponse(StatusCode statusCode) => StatusCode = statusCode;
    public SpinResponse(StatusCode statusCode, string? error) => (StatusCode, Error) = (statusCode, error);
    public SpinResponse(T? value) => (Value, StatusCode, HasValue) = value switch
    {
        null => (default, StatusCode.NotFound, false),
        var v => (v, StatusCode.OK, true),
    };
    public SpinResponse(T value, StatusCode statusCode) => (Value, StatusCode, HasValue) = (value, statusCode, value != null ? true : false);

    [Id(0)] public StatusCode StatusCode { get; }
    [Id(1)] public T? Value { get; }
    [Id(2)] public string? Error { get; }
    [Id(3)] public bool HasValue { get; }

    public object ValueObject => Value!;

    public static implicit operator SpinResponse<T>(T value) => new SpinResponse<T>(value);
}
