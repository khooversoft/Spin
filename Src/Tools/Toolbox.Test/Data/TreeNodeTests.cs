using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
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

        tree.Value.Should().BeNull();
        tree.Children.Count.Should().Be(4);
        tree.Children[0].Value.Should().Be("1");
        tree.Children[1].Value.Should().Be("2");
        tree.Children[2].Value.Should().Be("3");

        tree.Children[3].Value.Should().Be("4");
        tree.Children[3].Children.Count.Should().Be(2);
        tree.Children[3].Children[0].Value.Should().Be("4-1");
        tree.Children[3].Children[1].Value.Should().Be("4-2");
    }
}
