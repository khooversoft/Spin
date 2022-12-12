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


public class LiteralRule : TreeNode<IRule>, IRuleSingle
{
    public override string ToString() => $"[{this.GetType().Name}]";

    public IRuleValue? Match(IToken token) => token switch
    {
        TokenValue v => new LiteralRuleValue(v.Value),
        BlockToken v => new LiteralRuleValue(v.Value, v.BlockSignal),
        _ => null,
    };
}


public class LiteralRuleValue : TreeNode<IRuleValue>, IRuleValue
{
    public LiteralRuleValue(string value) => Value = value.NotEmpty();

    public LiteralRuleValue(string value, char blockSignal)
    {
        Value = value.NotEmpty();
        BlockSignal = blockSignal;
    }

    public string Value { get; }
    public char? BlockSignal { get; }

    public override string ToString() => $"[{this.GetType().Name}]: value={Value}";

}