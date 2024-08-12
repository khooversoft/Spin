using Toolbox.Types;

namespace Toolbox.LangTools;

public record SyntaxResponse
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public SyntaxTree SyntaxTree { get; init; } = null!;
}
