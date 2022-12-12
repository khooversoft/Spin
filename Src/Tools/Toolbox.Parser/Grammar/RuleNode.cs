using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Parser.Syntax;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Types.Structure;

namespace Toolbox.Parser.Grammar;

public class RuleNode : TreeNode<IRule>, IRuleSingle
{
    public RuleNode(string name) => Name = name.NotEmpty();

    public string Name { get; }
    public IRuleValue? Match(IToken _) => new RuleNodeValue(Name);

    public override string ToString() => $"[{this.GetType().Name}]: name={Name}";
}


public class RuleNodeValue : TreeNode<IRuleValue>, IRuleValue
{
    public RuleNodeValue(string name) => Name = name.NotEmpty();
    public string Name { get; }
    public override string ToString() => $"[{this.GetType().Name}]: name={Name}";
}
