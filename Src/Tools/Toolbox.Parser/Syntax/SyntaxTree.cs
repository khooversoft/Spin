using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Parser.Syntax;

public class SyntaxTree : List<ISyntaxNode>, ISyntaxNodeCollection
{
    public SyntaxTree()
    {
        Cursor = new SyntaxTreeCursor(this);
    }

    public SyntaxTreeCursor Cursor { get; set; }

    public override string ToString() => this.Select(x => x.ToString()).Join(",");

    public new SyntaxTree Add(ISyntaxNode value) => this.Action(_ => base.Add(value.SetParent(this)));
    public new SyntaxTree AddRange(IEnumerable<ISyntaxNode> values) => this.Action(_ => base.AddRange(values.Select(x => x.SetParent(this))));

    public static SyntaxTree operator +(SyntaxTree tree, ISyntaxNode value) => tree.Action(x => x.Add(value));
}


public class SyntaxTreeCursor
{
    private readonly SyntaxTree _parent;
    private ISyntaxNodeCollection? _current;

    internal SyntaxTreeCursor(SyntaxTree parent) => _parent = parent.NotNull();

    public ISyntaxNodeCollection Current
    {
        get => _current ?? _parent;
        set => _current = value is SyntaxTree ? null : value;
    }

    public SyntaxTree Add(ISyntaxNode value) => _parent.Action(_ => Current.Add(value));
    public SyntaxTree AddRange(IEnumerable<ISyntaxNode> values) => _parent.Action(_ => Current.AddRange(values));

    public SyntaxTree SetLast() => _parent.Action(_ => Current = (ISyntaxNodeCollection)Current.Last());
    public SyntaxTree SetParent() => _parent.Action(_ => _current = _current == null ? null : ((ISyntaxNode)_current).Parent);

    public static SyntaxTreeCursor operator +(SyntaxTreeCursor tree, ISyntaxNode value) => tree.Action(x => x.Add(value));
}
 
