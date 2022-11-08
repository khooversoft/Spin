using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Parser.Syntax;

public static class SyntaxNode
{
    public static ISyntaxNode Create<T>(T value) => new SyntaxNode<T>(value);
}


public class SyntaxNode<T> : List<ISyntaxNode>, ISyntaxNode, ISyntaxNodeCollection
{
    public SyntaxNode() { }

    public SyntaxNode(T value) => Value = value.NotNull();

    public ISyntaxNodeCollection? Parent { get; set; }
    public T? Value { get; }

    public new SyntaxNode<T> Add(ISyntaxNode value) => this.Action(_ => base.Add(value.SetParent(this)));
    public new SyntaxNode<T> AddRange(IEnumerable<ISyntaxNode> values) => this.Action(_ => base.AddRange(values.Select(x => x.SetParent(this))));

    public override string ToString() => this.Select(x => x.ToString()).Join(",");

    public static SyntaxNode<T> operator +(SyntaxNode<T> node, ISyntaxNode value) => node.Action(x => x.Add(value));
}
