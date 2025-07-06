using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

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

    private readonly ITestOutputHelper _outputHelper;

    public NodeInstructionDataTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task SingleNodeWithData()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeInstructionDataTest>();

        var nodeData = new NodeData { Name = "node8", Age = 40 };

        await CreateNode("node10", nodeData, graphTestClient);

        var getNode = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey("node10"))
            .AddDataName("entity")
            .Build();

        var readOption = await graphTestClient.Execute(getNode, context);
        readOption.IsOk().BeTrue();
        var read = readOption.Return();
        read.Nodes.Count.Be(1);
        read.DataLinks.Count.Be(1);

        var readNodeData = read.DataLinkToObject<NodeData>("entity");
        (nodeData == readNodeData).BeTrue();
    }

    [Fact]
    public async Task TwoNodeWithData()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(_map.Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<NodeInstructionDataTest>();

        var nodeData1 = new NodeData { Name = "node10", Age = 40 };
        await CreateNode("node10", nodeData1, graphTestClient);

        var nodeData2 = new NodeData { Name = "node11", Age = 55 };
        await CreateNode("node11", nodeData2, graphTestClient);

        var getNode = new SelectCommandBuilder()
            .AddNodeSearch()
            .AddDataName("entity")
            .Build();

        var readOption = await graphTestClient.Execute(getNode, NullScopeContext.Instance);
        readOption.IsOk().BeTrue();
        var read = readOption.Return();
        read.Nodes.Count.Be(9);
        read.DataLinks.Count.Be(2);

        var readNodeData1 = read.DataLinkToObject<NodeData>("entity", "node10");
        readNodeData1.IsOk().BeTrue();
        (nodeData1 == readNodeData1.Return()).BeTrue();

        var readNodeData2 = read.DataLinkToObject<NodeData>("entity", "node11");
        readNodeData2.IsOk().BeTrue();
        (nodeData2 == readNodeData2.Return()).BeTrue();

        var faileRead = read.DataLinkToObject<NodeData>("entity", "nodexx");
        faileRead.IsError().BeTrue();
    }

    private static async Task CreateNode(string nodeKey, NodeData nodeData, GraphHostService testClient)
    {
        var cmd = new NodeCommandBuilder()
            .SetNodeKey(nodeKey)
            .AddData("entity", nodeData)
            .Build();

        var newMapOption = await testClient.Execute(cmd, NullScopeContext.Instance);
        newMapOption.IsOk().BeTrue();
    }

    private record NodeData
    {
        public string Name { get; init; } = null!;
        public int Age { get; init; }
    }
}
