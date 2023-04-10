using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types.Structure;

public class Tree : List<ITreeNode>, ITreeParent
{
    public Tree() => Cursor = new TreeCursor(this);

    public TreeCursor Cursor { get; set; }

    public override string ToString() => this.Select(x => x.ToString()).Join(",");

    public new Tree Add(ITreeNode value) => this.Action(_ => base.Add(value.SetParent(this)));
    public new Tree AddRange(IEnumerable<ITreeNode> values) => this.Action(_ => base.AddRange(values.Select(x => x.SetParent(this))));

    public static Tree operator +(Tree tree, ITreeNode value) => tree.Action(x => x.Add(value));
}


public class TreeCursor
{
    private readonly Tree _parent;
    private ITreeParent? _current;

    internal TreeCursor(Tree parent) => _parent = parent.NotNull();

    public ITreeParent Current
    {
        get => _current ?? _parent;
        set => _current = value is Tree ? null : value;
    }

    public Tree Add(ITreeNode value)
    {
        switch (_current)
        {
            case null:
                _parent.Add(value);
                break;

            case ITreeNode v:
                v.Add(value);
                break;

            default:
                throw new InvalidOperationException($"Not valid current {_current?.GetType()?.Name ?? "<none>"}");
        }

        return _parent;
    }

    public Tree AddRange(IEnumerable<ITreeNode> values) => _parent.Action(_ => values.ForEach(x => Add(x)));

    public Tree SetLast() => _parent.Action(_ => Current = Current.Last());
    public Tree SetParent() => _parent.Action(_ => _current = _current == null ? null : ((ITreeNode)_current).Parent);

    public static TreeCursor operator +(TreeCursor tree, ITreeNode value) => tree.Action(x => x.Add(value));
}