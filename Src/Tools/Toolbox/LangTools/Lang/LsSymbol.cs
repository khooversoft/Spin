//using System.Diagnostics;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.LangTools;

//[DebuggerDisplay("Symbol={Symbol}, Name={Name}")]
//public class LsSymbol : ILangSyntax
//{
//    public LsSymbol(string symbol, bool optional)
//    {
//        Symbol = symbol.NotNull();
//        Name = symbol;
//        Optional = optional;
//    }

//    public LsSymbol(string symbol, string? name = null, bool optional = false)
//    {
//        Symbol = symbol.NotNull();
//        Name = name ?? Symbol;
//        Optional = optional;
//    }

//    public string Symbol { get; }
//    public string Name { get; }
//    public bool Optional { get; }

//    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? syntaxCursor)
//    {
//        syntaxCursor.NotNull();

//        if (!pContext.TokensCursor.TryGetValue(out var token)) return failStatus();

//        switch (token)
//        {
//            case TokenValue tokenValue when tokenValue.Value == Symbol:
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

//    public override string ToString() => $"{nameof(LsToken)}: Symbol={Symbol}, Name={Name}, Optional={Optional}";
//}
