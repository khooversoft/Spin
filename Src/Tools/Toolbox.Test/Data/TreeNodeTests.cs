using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data;

public class TreeNodeTests
{
    [Fact]
    public void NormalConstuction()
    {
        var tree = new TreeNode<string>
        {
            "1",
            "2",
            "3",
            new TreeNode<string>("4")
            {
                "4-1",
                "4-2"
            }
        };

        tree.Value.BeNull();
        tree.Children.Count.Be(4);
        tree.Children[0].Value.Be("1");
        tree.Children[1].Value.Be("2");
        tree.Children[2].Value.Be("3");

        tree.Children[3].Value.Be("4");
        tree.Children[3].Children.Count.Be(2);
        tree.Children[3].Children[0].Value.Be("4-1");
        tree.Children[3].Children[1].Value.Be("4-2");
    }
}
