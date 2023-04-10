using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Types.Structure;

namespace Toolbox.Parser.Grammar;

public enum LiteralType
{
    Symbol,
    String,
}

public class LiteralRule : TreeNode<IRule>, IRuleSingle
{

    public LiteralRule() => Type = LiteralType.Symbol;
    public LiteralRule(LiteralType type) => Type = type;

    private LiteralType Type { get; }
    public override string ToString() => $"[{this.GetType().Name}]";

    public IRuleValue? Match(IToken token) => token switch
    {
        TokenValue v when Type == LiteralType.Symbol => new LiteralRuleValue(LiteralType.Symbol, v.Value),
        BlockToken v when Type == LiteralType.String => new LiteralRuleValue(v.Value, v.BlockSignal),
        _ => null,
    };
}


public class LiteralRuleValue : TreeNode<IRuleValue>, IRuleValue
{
    public LiteralRuleValue(LiteralType type, string value)
    {
        Type = type;
        Value = value.NotEmpty();
    }

    public LiteralRuleValue(string value, char blockSignal)
    {
        Type = LiteralType.String;
        Value = value.NotEmpty();
        BlockSignal = blockSignal;
    }

    public LiteralType Type { get; }
    public string Value { get; }
    public char? BlockSignal { get; }

    public override string ToString() => $"[{this.GetType().Name}]: value={Value}";

}