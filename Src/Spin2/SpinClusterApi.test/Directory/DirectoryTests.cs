using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Directory;
using SpinClusterApi.test.Application;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClusterApi.test.Directory;

public class DirectoryTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public DirectoryTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    [Fact]
    public async Task SingleNode()
    {
        const string nodeKey = "node1";
        var dirClient = _cluster.ServiceProvider.GetRequiredService<DirectoryClient>();

        var query = new DirectoryQuery($"(Key={nodeKey})");

        var r = await dirClient.Remove("(Key=*)", _context);
        r.IsBadRequest().Should().BeFalse();

        var node = new GraphNode
        {
            Key = nodeKey,
            Tags = "t1",
        };

        var addOption = await dirClient.AddNode(node, _context);
        addOption.IsOk().Should().BeTrue();

        var getNodeOption = await dirClient.Query(query, _context);
        getNodeOption.IsOk().Should().BeTrue();

        GraphQueryResult response = getNodeOption.Return();
        response.Items.Count.Should().Be(1);
        response.Edges().Count.Should().Be(0);
        response.Nodes().First().Key.Should().Be(nodeKey);
        response.Nodes().First().Tags.ToString().Should().Be("t1");

        getNodeOption = await dirClient.Query(new DirectoryQuery("(Key=*)"), _context);
        getNodeOption.IsOk().Should().BeTrue();
        response = getNodeOption.Return();
        response.Nodes().Count.Should().Be(1);
        response.Edges().Count.Should().Be(0);
        response.Nodes().First().Key.Should().Be(nodeKey);
        response.Nodes().First().Tags.ToString().Should().Be("t1");

        getNodeOption = await dirClient.Query(new DirectoryQuery("(Tags=t1)"), _context);
        getNodeOption.IsOk().Should().BeTrue();
        response = getNodeOption.Return();
        response.Nodes().Count.Should().Be(1);
        response.Edges().Count.Should().Be(0);
        response.Nodes().First().Key.Should().Be(nodeKey);
        response.Nodes().First().Tags.ToString().Should().Be("t1");

        var deleteOption = await dirClient.Remove(query, _context);
        deleteOption.IsOk().Should().BeTrue();

        getNodeOption = await dirClient.Query(query, _context);
        getNodeOption.IsOk().Should().BeTrue();
        getNodeOption.Return().Nodes().Count.Should().Be(0);
        getNodeOption.Return().Edges().Count.Should().Be(0);
    }

    [Fact]
    public async Task SingleNodeUpdateTag()
    {
        var dirClient = _cluster.ServiceProvider.GetRequiredService<DirectoryClient>();
        var addNode = new GraphNode { Key = "node1", Tags = "t1;name=fred" };

        var query = new DirectoryQuery("(Key = node1)");

        var r = await dirClient.Remove("(Key=*)", _context);
        r.IsBadRequest().Should().BeFalse();

        var addOption = await dirClient.AddNode(addNode, _context);
        addOption.IsOk().Should().BeTrue();

        var getNodeOption = await dirClient.Query(query, _context);
        getNodeOption.IsOk().Should().BeTrue();

        GraphQueryResult response = getNodeOption.Return();
        response.Nodes().Count.Should().Be(1);
        response.Edges().Count.Should().Be(0);
        response.Nodes().First().Key.Should().Be("node1");
        response.Nodes().First().Tags.ToString().Should().Be("t1;name=fred");

        var updateResponse = await dirClient.UpdateNode(new DirectoryNodeUpdate { Key = "node1", UpdateTags = "name=adam" }, _context);
        updateResponse.IsOk().Should().BeTrue();

        getNodeOption = await dirClient.Query(query, _context);
        getNodeOption.IsOk().Should().BeTrue();
        response = getNodeOption.Return();
        response.Nodes().Count.Should().Be(1);
        response.Edges().Count.Should().Be(0);
        response.Nodes().First().Key.Should().Be("node1");
        response.Nodes().First().Tags.ToString().Should().Be("t1;name=adam");

        getNodeOption = await dirClient.Query(new DirectoryQuery("(Tags=name)"), _context);
        getNodeOption.IsOk().Should().BeTrue();
        response = getNodeOption.Return();
        response.Nodes().Count.Should().Be(1);
        response.Edges().Count.Should().Be(0);
        response.Nodes().First().Key.Should().Be("node1");
        response.Nodes().First().Tags.ToString().Should().Be("t1;name=adam");

        var deleteOption = await dirClient.Remove(query, _context);
        deleteOption.IsOk().Should().BeTrue();

        getNodeOption = await dirClient.Query(query, _context);
        getNodeOption.IsOk().Should().BeTrue();
        getNodeOption.Return().Nodes().Count.Should().Be(0);
        getNodeOption.Return().Edges().Count.Should().Be(0);
    }

    [Fact]
    public async Task MultipleNodes()
    {
        const int count = 3;
        var dirClient = _cluster.ServiceProvider.GetRequiredService<DirectoryClient>();

        var r = await dirClient.Remove("(Key=*)", _context);
        r.IsBadRequest().Should().BeFalse();

        var addRecords = Enumerable.Range(0, count)
            .Select(x => new GraphNode { Key = $"node_{x}", Tags = "t1" })
            .ToArray();

        foreach (var node in addRecords)
        {
            var addOption = await dirClient.AddNode(node, _context);
            addOption.IsOk().Should().BeTrue();
        }

        var getNodeOption = await dirClient.Query("(Key = *)", _context);
        getNodeOption.IsOk().Should().BeTrue();
        GraphQueryResult response = getNodeOption.Return();
        response.Nodes().Count.Should().Be(count);
        response.Edges().Count.Should().Be(0);

        foreach (var node in addRecords)
        {
            getNodeOption = await dirClient.Query($"(Key = {node.Key})", _context);
            getNodeOption.IsOk().Should().BeTrue();
            response = getNodeOption.Return();
            response.Nodes().Count.Should().Be(1);
            response.Edges().Count.Should().Be(0);
            response.Nodes().First().Key.Should().Be(node.Key);
            response.Nodes().First().Tags.ToString().Should().Be("t1");
        }

        getNodeOption = await dirClient.Query("(Tags = t1)", _context);
        getNodeOption.IsOk().Should().BeTrue();
        response = getNodeOption.Return();
        response.Nodes().Count.Should().Be(count);
        response.Edges().Count.Should().Be(0);

        var zip = addRecords.Zip(response.Nodes().OrderBy(x => x.Key)).ToArray();
        zip.All(x => x.First.Key == x.Second.Key && x.First.Tags == x.Second.Tags).Should().BeTrue();

        foreach (var node in addRecords)
        {
            var query = new DirectoryQuery($"(Key = {node.Key})");
            var deleteOption = await dirClient.Remove(query, _context);
            deleteOption.IsOk().Should().BeTrue();

            getNodeOption = await dirClient.Query(query, _context);
            getNodeOption.IsOk().Should().BeTrue();
            getNodeOption.Return().Nodes().Count.Should().Be(0);
            getNodeOption.Return().Edges().Count.Should().Be(0);
        }
    }

    [Fact]
    public async Task TwoNodeAndEdge()
    {
        var dirClient = _cluster.ServiceProvider.GetRequiredService<DirectoryClient>();
        var addNodes = new[] { new GraphNode { Key = "node1" }, new GraphNode { Key = "node2" } };
        var addEdge = new GraphEdge { FromKey = "node1", ToKey = "node2", Tags = "t1" };

        var r = await dirClient.Remove("(Key=*)", _context);
        r.IsBadRequest().Should().BeFalse();

        await addNodes.ForEachAsync(async x =>
        {
            var addOption = await dirClient.AddNode(x, _context);
            addOption.IsOk().Should().BeTrue();
        });

        var addEdgeOption = await dirClient.AddEdge(addEdge, _context);
        addEdgeOption.IsOk().Should().BeTrue();

        var queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=node1;ToKey=node2]"), _context);
        queryOption.IsOk().Should().BeTrue();
        GraphQueryResult response = queryOption.Return();
        test(response);

        queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=node1]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response);

        queryOption = await dirClient.Query(new DirectoryQuery("[ToKey=node2]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response);

        queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=*]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response);

        queryOption = await dirClient.Query(new DirectoryQuery("[ToKey=*]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response);

        await addNodes.ForEachAsync(async x =>
        {
            var removeOption = await dirClient.Remove(new DirectoryQuery($"(Key = {x.Key})"), _context);
            removeOption.IsOk().Should().BeTrue();
        });

        queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=*]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        response.Nodes().Count.Should().Be(0);
        response.Edges().Count.Should().Be(0);

        void test(GraphQueryResult response)
        {
            response.Nodes().Count.Should().Be(0);
            response.Edges().Count.Should().Be(1);
            response.Edges().First().FromKey.Should().Be("node1");
            response.Edges().First().ToKey.Should().Be("node2");
        }
    }

    [Fact]
    public async Task TwoNodeAndTwoEdgesDirected()
    {
        var dirClient = _cluster.ServiceProvider.GetRequiredService<DirectoryClient>();
        var addNodes = new[] { new GraphNode { Key = "node1" }, new GraphNode { Key = "node2" } };
        var addEdges = new[]
        {
            new GraphEdge { FromKey = "node1", ToKey = "node2", Tags = "t1" },
            new GraphEdge { FromKey = "node2", ToKey = "node1", Tags = "t1" },
        };

        var r = await dirClient.Remove("(Key=*)", _context);
        r.IsBadRequest().Should().BeFalse();

        await addNodes.ForEachAsync(async x =>
        {
            var addOption = await dirClient.AddNode(x, _context);
            addOption.IsOk().Should().BeTrue();
        });

        await addEdges.ForEachAsync(async x =>
        {
            var addEdgeOption = await dirClient.AddEdge(x, _context);
            addEdgeOption.IsOk().Should().BeTrue();
        });

        var queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=node1;ToKey=node2]"), _context);
        queryOption.IsOk().Should().BeTrue();
        GraphQueryResult response = queryOption.Return();
        test(response, addEdges[0]);

        queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=node1]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response, addEdges[0]);

        queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=node2]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response, addEdges[1]);

        queryOption = await dirClient.Query(new DirectoryQuery("[ToKey=node2]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response, addEdges[0]);

        queryOption = await dirClient.Query(new DirectoryQuery("[ToKey=node1]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response, addEdges[1]);

        queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=*]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response, addEdges[0], addEdges[1]);

        queryOption = await dirClient.Query(new DirectoryQuery("[ToKey=*]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        test(response, addEdges[0], addEdges[1]);

        await addNodes.ForEachAsync(async x =>
        {
            var removeOption = await dirClient.Remove(new DirectoryQuery($"(Key = {x.Key})"), _context);
            removeOption.IsOk().Should().BeTrue();
        });

        queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=*]"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        response.Nodes().Count.Should().Be(0);
        response.Edges().Count.Should().Be(0);

        void test(GraphQueryResult response, params GraphEdge[] edges)
        {
            response.Nodes().Count.Should().Be(0);
            response.Edges().Count.Should().Be(edges.Length);

            var e = response.Edges().OrderBy(x => x.FromKey).ToArray();

            edges.WithIndex().ForEach(x =>
            {
                e[x.Index].FromKey.Should().Be(x.Item.FromKey);
                e[x.Index].ToKey.Should().Be(x.Item.ToKey);
            });
        }
    }

    [Fact]
    public async Task TwoEdgesDelete()
    {
        var dirClient = _cluster.ServiceProvider.GetRequiredService<DirectoryClient>();
        var addNodes = new[]
        {
            new GraphNode { Key = "node1" },
            new GraphNode { Key = "node2" }
        };

        var addEdges = new[]
        {
            new GraphEdge { FromKey = "node1", ToKey = "node2", Tags = "t1" },
            new GraphEdge { FromKey = "node2", ToKey = "node1", Tags = "t1" },
        };

        var r = await dirClient.Remove("(Key=*)", _context);
        r.IsBadRequest().Should().BeFalse();

        await addNodes.ForEachAsync(async x =>
        {
            var addOption = await dirClient.AddNode(x, _context);
            addOption.IsOk().Should().BeTrue();
        });

        await addEdges.ForEachAsync(async x =>
        {
            var addEdgeOption = await dirClient.AddEdge(x, _context);
            addEdgeOption.IsOk().Should().BeTrue();
        });

        var queryOption = await dirClient.Query(new DirectoryQuery("(Key=*) a1 -> [FromKey=*] a2"), _context);
        queryOption.IsOk().Should().BeTrue();
        GraphQueryResult response = queryOption.Return();
        response.Nodes().Count.Should().Be(0);
        response.Edges().Count.Should().Be(2);
        response.AliasEdge("a2").Count.Should().Be(2);
        response.AliasNode("a1").Count.Should().Be(2);

        queryOption = await dirClient.Query(new DirectoryQuery("[FromKey=*] a1"), _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        response.Nodes().Count.Should().Be(0);
        response.Edges().Count.Should().Be(2);
        response.AliasEdge("a1").Count.Should().Be(2);

        await addEdges.ForEachAsync(async x =>
        {
            var removeOption = await dirClient.Remove($"[FromKey={x.FromKey}]", _context);
            removeOption.IsOk().Should().BeTrue();
        });

        queryOption = await dirClient.Query("(Key=*) a1", _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        response.Nodes().Count.Should().Be(2);
        response.Edges().Count.Should().Be(0);
        response.AliasNode("a1").Count.Should().Be(2);

        await addNodes.ForEachAsync(async x =>
        {
            var removeOption = await dirClient.Remove($"(Key={x.Key})", _context);
            removeOption.IsOk().Should().BeTrue();
        });

        queryOption = await dirClient.Query("(key=*)", _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        response.Nodes().Count.Should().Be(0);
        response.Edges().Count.Should().Be(0);

        queryOption = await dirClient.Query("[fromKey=*]", _context);
        queryOption.IsOk().Should().BeTrue();
        response = queryOption.Return();
        response.Nodes().Count.Should().Be(0);
        response.Edges().Count.Should().Be(0);
    }
}
