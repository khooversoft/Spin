using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.LangTools.Pattern;


[DebuggerDisplay("Count={Children.Count}")]
public class PatternNodes : LangBase<PatternNode>
{
    public PatternNodes() { }
    public PatternNodes(IEnumerable<PatternNode> nodes) => AddRange(nodes);

    public PatternNode this[int index] => Children[index];

    public static PatternNodes operator +(PatternNodes sequence, PatternNode value) => sequence.Action(x => x.Add(value));
    public static PatternNodes operator +(PatternNodes sequence, IEnumerable<PatternNode> values) => sequence.Action(x => x.AddRange(values));
}


[DebuggerDisplay("SyntaxNode={SyntaxNode}")]
public class PatternNode
{
    public PatternNode(IPatternSyntax syntaxNode, string value)
    {
        syntaxNode.NotNull();
        value.NotEmpty();

        SyntaxNode = syntaxNode;
        Value = value;
    }

    public IPatternSyntax SyntaxNode { get; }
    public string Value { get; }

    public override string ToString() => $"{typeof(LangNode)}: SyntaxNode={SyntaxNode}, Value={Value}";
}
