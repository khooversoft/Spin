//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Extensions;
//using Toolbox.Tokenizer.Token;
//using Toolbox.Types;

//namespace Toolbox.Parser.Grammar;

//public class IntRule : IRule
//{
//    public int Value { get; }

//    public IntRule(int value) => Value = value;

//    public static IReadOnlyList<IntRule>? Match(Cursor<IToken> tokenCursor) => (found: tokenCursor.TryNextValue(out IToken? token), token) switch
//    {
//        (true, TokenValue) v => int.TryParse(v.token!.Value, out int intValue) switch
//        {
//            true => new IntRule(intValue).ToEnumerable().ToArray(),
//            _ => null,
//        },

//        _ => null,
//    };
//}
