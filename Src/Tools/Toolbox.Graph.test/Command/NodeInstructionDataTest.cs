using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class NodeInstructionDataTest
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=27"),
        new GraphNode("node3", tags: "name=lop,lang=java"),
        new GraphNode("node4", tags: "name=josh,age=32"),
        new GraphNode("node5", tags: "name=ripple,lang=java"),
        new GraphNode("node6", tags: "name=peter,age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", edgeType: "et1", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node5", edgeType: "et1", tags: "created"),
        new GraphEdge("node4", "node3", edgeType : "et1", tags: "created"),
    };

    [Fact]
    public async Task SingleNodeWithData()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);

        var nodeData = new NodeData { Name = "node8", Age = 40 };

        await CreateNode("node10", nodeData, testClient);

        var getNode = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("node10"))
            .AddDataName("entity")
            .Build();

        var readOption = await testClient.Execute(getNode, NullScopeContext.Default);
        readOption.IsOk().Should().BeTrue();
        var read = readOption.Return();
        read.Nodes.Count.Should().Be(1);
        read.DataLinks.Count.Should().Be(1);

        var readNodeData = read.DataLinkToObject<NodeData>("entity");
        (nodeData == readNodeData).Should().BeTrue();
    }

    [Fact]
    public async Task TwoNodeWithData()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);

        var nodeData1 = new NodeData { Name = "node10", Age = 40 };
        await CreateNode("node10", nodeData1, testClient);

        var nodeData2 = new NodeData { Name = "node11", Age = 55 };
        await CreateNode("node11", nodeData2, testClient);

        var getNode = new SelectCommandBuilder()
            .AddNodeSearch()
            .AddDataName("entity")
            .Build();

        var readOption = await testClient.Execute(getNode, NullScopeContext.Default);
        readOption.IsOk().Should().BeTrue();
        var read = readOption.Return();
        read.Nodes.Count.Should().Be(9);
        read.DataLinks.Count.Should().Be(2);

        var readNodeData1 = read.DataLinkToObject<NodeData>("entity", "node10");
        readNodeData1.IsOk().Should().BeTrue();
        (nodeData1 == readNodeData1.Return()).Should().BeTrue();

        var readNodeData2 = read.DataLinkToObject<NodeData>("entity", "node11");
        readNodeData2.IsOk().Should().BeTrue();
        (nodeData2 == readNodeData2.Return()).Should().BeTrue();

        var faileRead = read.DataLinkToObject<NodeData>("entity", "nodexx");
        faileRead.IsError().Should().BeTrue();
    }

    private static async Task CreateNode(string nodeKey, NodeData nodeData, GraphTestClient testClient)
    {
        var cmd = new NodeCommandBuilder()
            .SetNodeKey(nodeKey)
            .AddData("entity", nodeData)
            .Build();

        var newMapOption = await testClient.Execute(cmd, NullScopeContext.Default);
        newMapOption.IsOk().Should().BeTrue();
    }

    private record NodeData
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }
}
