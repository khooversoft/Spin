using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tokenizer.Token;
using Toolbox.Extensions;

namespace Toolbox.Parser.Grammar;

public enum DataType
{
    None,
    String,
    Int,
}

public class DataTypeRule : IRule
{
    private static IReadOnlyList<(DataType DataType, string MatchToValue)> _dataTypeTextList = new[]
    {
        (DataType.String, "string"),
        (DataType.Int, "int"),
    };

    public DataTypeRule(DataType dataType) => Type = dataType;

    public DataType Type { get; }

    public static DataTypeRule? Match(IToken token) => token switch
    {
        TokenValue v => _dataTypeTextList.FirstOrDefault(x => x.MatchToValue == v) switch
        {
            (DataType, string) v1 => new DataTypeRule(v1.DataType),
            _ => null,
        },

        _ => null,
    };
}
