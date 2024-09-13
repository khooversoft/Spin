//using System.Diagnostics;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.LangTools;

//[DebuggerDisplay("Token={Token}, Name={Name}")]
//public class LsToken : ILangSyntax
//{
//    public LsToken(string token, bool optional)
//    {
//        Token = token.NotNull();
//        Name = Token;
//        Optional = optional;
//    }

//    public LsToken(string symbol, string? name = null, bool optional = false)
//    {
//        Token = symbol.NotNull();
//        Name = name ?? symbol;
//        Optional = optional;
//    }

//    public string Token { get; }
//    public string Name { get; }
//    public bool Optional { get; }

//    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? syntaxCursor)
//    {
//        syntaxCursor.NotNull();

//        if (!pContext.TokensCursor.TryGetValue(out var token)) return failStatus();

//        switch (token)
//        {
//            case TokenValue tokenValue when tokenValue.Value == Token:
//                return new LangNodes() + new LangNode(syntaxCursor.Current, tokenValue.Value);

//            default:
//                if (Optional) pContext.TokensCursor.Index--;
//                return (failStatus(), $"Syntax error: unknown token={token.Value}");
//        }

//        StatusCode failStatus() => Optional switch
//        {
//            false => StatusCode.BadRequest,
//            true => StatusCode.NoContent,
//        };
//    }

//    public override string ToString() => $"{nameof(LsToken)}: Symbol={Token}, Name={Name}, Optional={Optional}";
//}
