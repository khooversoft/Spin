using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Toolbox.Extensions;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Types.Structure;

namespace Toolbox.Parser.Grammar;

public class TokenRule : TreeNode<IRule>, IRuleSingle
{
    public TokenRule(string value) => Value = value;
    public string Value { get; }

    public override string ToString() => $"[{this.GetType().Name}]: value={Value}";

    public IRuleValue? Match(IToken token) => token switch
    {
        TokenValue v when v.Value.EqualsIgnoreCase(Value) => new TokenRuleValue(v.Value),
        _ => null,
    };
}


public class TokenRuleValue : TreeNode<IRuleValue>, IRuleValue
{
    public TokenRuleValue(string value) => Value = value.NotEmpty();
    public string Value { get; }

    public override string ToString() => $"[{this.GetType().Name}]: value={Value}";
}