using System.Diagnostics;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

[DebuggerDisplay("Name={Name}")]
public class LsValue : ILangSyntax
{
    public LsValue(bool optional = false) => Optional = optional;

    public LsValue(string? name, bool optional = false)
    {
        Name = name;
        Optional = optional;
    }

    public string? Name { get; }
    public bool Optional { get; }


    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? syntaxCursor)
    {
        var result = pContext.RunAndLog(nameof(LsValue), Name, () => InternalProcess(pContext, syntaxCursor));
        return result;
    }

    public Option<LangNodes> InternalProcess(LangParserContext pContext, Cursor<ILangSyntax>? syntaxCursor)
    {
        syntaxCursor.NotNull();

        if (!pContext.TokensCursor.TryNextValue(out var token)) return failStatus();

        switch (token)
        {
            case TokenValue tokenValue when !tokenValue.IsSyntaxToken:
                return new LangNodes() + new LangNode(syntaxCursor.Current, tokenValue.Value);

            case BlockToken blockToken:
                return new LangNodes() + new LangNode(syntaxCursor.Current, blockToken.Value);

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
    public override string ToString() => $"{nameof(LsValue)}: Name={Name}";

}
