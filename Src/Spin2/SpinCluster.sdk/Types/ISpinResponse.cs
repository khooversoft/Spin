using Toolbox.Types;

namespace SpinCluster.sdk.Types;

public interface ISpinResponse
{
    StatusCode StatusCode { get; }
    string? Error { get; }
}

public interface ISpinResponseWithValue : ISpinResponse
{
    bool HasValue { get; }
    object ValueObject { get; }
}
