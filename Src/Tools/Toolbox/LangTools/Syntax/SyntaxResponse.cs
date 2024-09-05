using Toolbox.Types;

namespace Toolbox.LangTools;

public record SyntaxResponse
{
    public Option Status { get; init; }
    public SyntaxTree SyntaxTree { get; init; } = null!;
}
