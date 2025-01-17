using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Builders;

public class DeleteCommandBuilderTests
{
    [Fact]
    public void DeleteNode()
    {
        var graphQuery = new DeleteCommandBuilder()
            .SetNodeKey("nodeKey1")
            .Build();

        graphQuery.Should().Be("delete node key=nodeKey1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
    }

    [Fact]
    public void DeleteNodeIfExist()
    {
        var graphQuery = new DeleteCommandBuilder()
            .SetNodeKey("nodeKey1")
            .SetIfExist()
            .Build();

        graphQuery.Should().Be("delete node ifexist key=nodeKey1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Default);
        parse.Status.IsOk().Should().BeTrue();
    }
}
