//using System.Diagnostics;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace Toolbox.LangTools;


//[DebuggerDisplay("Count={Children.Count}")]
//public class LangNodes : LangBase<LangNode>
//{
//    public static LangNodes operator +(LangNodes sequence, LangNode value) => sequence.Action(x => x.Add(value));
//    public static LangNodes operator +(LangNodes sequence, IEnumerable<LangNode> values) => sequence.Action(x => x.AddRange(values));
//}


//[DebuggerDisplay("SyntaxNode={SyntaxNode}, Value={Value}")]
//public class LangNode : LangBase<LangNode>
//{
//    public LangNode(ILangSyntax syntaxNode, string value)
//    {
//        syntaxNode.NotNull();
//        value.NotEmpty();

//        SyntaxNode = syntaxNode;
//        Value = value;
//    }

//    public ILangSyntax SyntaxNode { get; }
//    public string Value { get; }

//    public override string ToString() => $"{typeof(LangNode)}: SyntaxNode={SyntaxNode}, Value={Value}";
//}
