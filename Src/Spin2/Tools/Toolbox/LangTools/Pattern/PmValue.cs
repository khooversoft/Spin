using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public class PmValue : IPatternSyntax
{
    public PmValue(string? name = null, bool optional = false)
    {
        Name = name;
        Optional = optional;
    }

    public string? Name { get; }
    public bool Optional { get; }

    public Option<Sequence<IPatternSyntax>> Process(PatternContext pContext)
    {
        pContext.NotNull();

        if (!pContext.TokensCursor.TryNextValue(out var token)) return failStatus();
        if (!pContext.Syntax.TryNextValue(out IPatternSyntax? syntax)) return failStatus();

        switch (token)
        {
            case TokenValue tokenValue when !tokenValue.IsSyntaxToken:
                return new Sequence<IPatternSyntax>() + syntax;

            case BlockToken:
                return new Sequence<IPatternSyntax>() + syntax;

            default:
                if (Optional) pContext.TokensCursor.Index--;
                return (failStatus(), $"Syntax error: unknown token={token.Value}");
        }

        StatusCode failStatus() => Optional switch
        {
            false => StatusCode.BadRequest,
            true => StatusCode.NoContent,
        };
    }
}
