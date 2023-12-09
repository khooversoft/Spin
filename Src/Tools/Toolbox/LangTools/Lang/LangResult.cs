using Toolbox.Types;

namespace Toolbox.LangTools;

public record LangResult
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public string RawData { get; init; } = null!;
    public LangNodes? LangNodes { get; init; }
    public string? MaxTokens { get; init; }

    public bool IsError() => StatusCode.IsError();
    public bool IsOk() => StatusCode.IsOk();
}
