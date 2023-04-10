using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types.Structure;


public class TreeNode<T> : List<ITreeNode>, ITreeNode
    where T : ITreeNode
{
    public TreeNode() { }

    public ITreeParent? Parent { get; set; }

    public new TreeNode<T> Add(ITreeNode value) => this.Action(_ => base.Add(value.SetParent(this)));
    public new TreeNode<T> AddRange(IEnumerable<ITreeNode> values) => this.Action(_ => base.AddRange(values.Select(x => x.SetParent(this))));

    public override string ToString() => this.Select(x => x.ToString()).Join(",");

    public static TreeNode<T> operator +(TreeNode<T> node, ITreeNode value) => node.Action(x => x.Add(value));
    public static TreeNode<T> operator +(TreeNode<T> node, ITreeNode[] values) => node.Action(x => x.AddRange(values));
}
