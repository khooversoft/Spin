using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types.Structure;


public interface ITreeParent : IEnumerable<ITreeNode>
{
    int Count{ get; }
}

public interface ITreeNode : ITreeParent
{
    ITreeParent? Parent { get; set; }
    void Add(ITreeNode value);
    void AddRange(IEnumerable<ITreeNode> values);

    //public static ITreeNode operator +(ITreeNode left, ITreeNode right)
    //{
    //    left.Add(right);
    //    return left;
    //}

    //public static ITreeNode operator +(ITreeNode left, IEnumerable<ITreeNode> right)
    //{
    //    left.AddRange(right);
    //    return left;
    //}
}


public static class TreeNodeExtensions
{
    public static ITreeNode SetParent(this ITreeNode node, ITreeParent parent) => node.Action(x => x.Parent = parent);
}
