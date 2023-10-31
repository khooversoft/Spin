using System.Diagnostics;
using Toolbox.Types;

namespace Toolbox.LangTools;

[DebuggerDisplay("Name={Name}")]
public class LsValue : ILangSyntax
{
    public LsValue(string? name)
    {
        Name = name;
    }

    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax> syntaxCursor)
    {
        if (!pContext.TokensCursor.TryNextValue(out var token)) return StatusCode.BadRequest;

        switch (token)
        {
            case TokenValue tokenValue when !tokenValue.IsSyntaxToken:
                return new LangNodes() + new LangNode(syntaxCursor.Current, tokenValue.Value);

            case BlockToken blockToken:
                return new LangNodes() + new LangNode(syntaxCursor.Current, blockToken.Value);

            default:
                return (StatusCode.BadRequest, $"Syntax error: unknown token={token.GetType().FullName}");
        }
    }
}
