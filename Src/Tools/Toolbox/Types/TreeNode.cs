using System.Collections;
using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

[DebuggerDisplay("Value={Value}, IsRoot={IsRoot}, IsLeaf={IsLeaf}")]
public class TreeNode<T> : IEnumerable<TreeNode<T>>
{
    public TreeNode() { }
    public TreeNode(T? value) { Value = value; }
    public TreeNode(TreeNode<T> parent, T? value) => (Parent, Value) = (parent.NotNull(), value);

    public TreeNode(TreeNode<T> parent, T? value, IEnumerable<TreeNode<T>> children)
    {
        Parent = parent.NotNull();
        Value = value;
        Children = children.ToList();
    }

    public TreeNode<T>? Parent { get; }
    public T? Value { get; }
    public List<TreeNode<T>> Children { get; } = new List<TreeNode<T>>();
    public bool IsRoot => Parent == null;
    public bool IsLeaf => Children.Count == 0;

    public TreeNode<T> Add(T value)
    {
        var node = new TreeNode<T>(this, value);
        Children.Add(node);
        return node;
    }

    public TreeNode<T> Add(TreeNode<T> node)
    {
        node = node.NotNull().WithParent(this);
        Children.Add(node);
        return node;
    }

    public void Remove()
    {
        Parent.Assert(x => x != null, "Parent is is null");
        if (Parent == null) throw new InvalidOperationException("Parent is null");

        Parent.Children.Remove(this);
    }

    public TreeNode<T> WithParent(TreeNode<T> parent) => new TreeNode<T>(parent, Value, Children);

    public IEnumerator<TreeNode<T>> GetEnumerator() => Children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static TreeNode<T> operator +(TreeNode<T> subject, T value) => subject.Action(x => x.Add(value));
    public static TreeNode<T> operator -(TreeNode<T> subject) => subject.Action(x => x.Remove());
}