using FluentAssertions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class SyntaxElementsTests
{
    [Fact]
    public void SyntaxNode_Equals()
    {
        var node1 = new SyntaxNode
        {
            Name = "Node1",
            Children = new Sequence<ISyntaxNode>
            {
                new TerminalNode { Name = "Terminal1", Text = "Text1" },
                new TerminalNode { Name = "Terminal2", Text = "Text2" }
            }
        };

        var node2 = new SyntaxNode
        {
            Name = "Node1",
            Children = new Sequence<ISyntaxNode>
            {
                new TerminalNode { Name = "Terminal1", Text = "Text1" },
                new TerminalNode { Name = "Terminal2", Text = "Text2" }
            }
        };

        (node1 == node2).Should().BeTrue();
        node1.Equals(node2).Should().BeTrue();
    }

    [Fact]
    public void TerminalNode_Equals()
    {
        var node1 = new TerminalNode
        {
            Name = "Node1",
            Text = "Text1"
        };

        var node2 = new TerminalNode
        {
            Name = "Node1",
            Text = "Text1"
        };

        (node1 == node2).Should().BeTrue();
        node1.Equals(node2).Should().BeTrue();
    }
}
