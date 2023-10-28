using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Tokenizer.Tree;

//public interface ILangNode : ILangTree
//{
//}


public class LangNode : LangBase<LangNode>, ILangTree
{
    public LangNode(ILangTree parent, ILangSyntax syntaxNode, string value)
        : base(syntaxNode.Type)
    {
        parent.NotNull();
        syntaxNode.NotNull();
        syntaxNode.Type.Assert(x => x.IsEnumValid(), x => $"Invalid enum {x}");
        value.NotEmpty();

        Parent = parent;
        SyntaxNode = syntaxNode;
        Value = value;
    }

    public ILangTree Parent { get; }
    public ILangSyntax SyntaxNode { get; }
    public string Value { get; }
}
