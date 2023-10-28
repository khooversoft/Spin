//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tokenizer.Token;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Tokenizer.Tree;

//public class TokenNodeParser
//{
//    private readonly record struct ParserContext
//    {
//        public required TokenNode SyntaxCurrent { get; init; }
//        public required TokenNode CurrentNode { get; init; }
//        public required Cursor<IToken> Tokens { get; init; }
//    }

//    public static TokenNode Parse(TokenNode syntax, IReadOnlyList<IToken> tokens)
//    {
//        syntax.NotNull();
//        tokens.NotNull();

//        var pContext = new ParserContext
//        {
//            SyntaxCurrent = syntax,
//            CurrentNode = TN.CreateRoot(),
//            Tokens = tokens.ToCursor(),
//        };

//        while (pContext.Tokens.TryNextValue(out var token))
//        {
//            switch (token)
//            {
//                case TokenValue tokenValue:
//                    //var tokenOption = pContext.SyntaxCurrent
//                    break;
//            }
//        }
//    }
//}
