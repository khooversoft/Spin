//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tokenizer.Token;
//using Toolbox.Extensions;
//using Toolbox.Types.Structure;
//using Toolbox.Tools;

//namespace Toolbox.Parser.Grammar;

//public enum DataType
//{
//    None,
//    String,
//    Int,
//}

//public class DataTypeRule : TreeNode<IRule>, IRuleSingle
//{
//    private static Dictionary<string, DataType> _map = new(StringComparer.OrdinalIgnoreCase)
//    {
//        ["string"] = DataType.String,
//        ["int"] = DataType.Int,
//    };

//    public IRuleValue? Match(IToken token) => token switch
//    {
//        TokenValue v => _map.TryGetValue(v.Value, out var value) switch
//        {
//            true => new DataTypeRuleValue(value),
//            _ => null,
//        },

//        _ => null,
//    };
//}


//public class DataTypeRuleValue : TreeNode<IRuleValue>, IRuleValue
//{
//    public DataTypeRuleValue(DataType dataType) => Value = dataType;
//    public DataType Value { get; }
//}