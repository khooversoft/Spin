using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphLifecycleTest
{
    [Fact]
    public async Task SingleNode()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

        Option<GraphQueryResults> addResult = await testClient.ExecuteBatch("add node key=node1, t1,t2=v1;", NullScopeContext.Instance);
        addResult.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Instance)).ThrowOnError().Return().Action(x =>
        {
            x.Status.IsOk().Should().BeTrue();
            x.Items.Length.Should().Be(1);
            x.Items.OfType<GraphNode>().First().Action(x =>
            {
                x.Key.Should().Be("node1");
                x.Tags.ToTagsString().Should().Be("t1,t2=v1");
            });
        });

        Option<GraphQueryResults> removeResult = await testClient.ExecuteBatch("delete (key=node1);", NullScopeContext.Instance);
        removeResult.IsOk().Should().BeTrue(removeResult.ToString());
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Instance)).ThrowOnError().Return().Action(x =>
        {
            x.Status.IsOk().Should().BeTrue();
            x.Items.Length.Should().Be(0);
        });
    }

    [Fact]
    public async Task TwoNodes()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

        Option<GraphQueryResults> addResult1 = await testClient.ExecuteBatch("add node key=node1, t1,t2=v1;", NullScopeContext.Instance);
        addResult1.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);

        Option<GraphQueryResults> addResult2 = await testClient.ExecuteBatch("add node key=node2, t10,t20=v10;", NullScopeContext.Instance);
        addResult2.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Instance)).ThrowOnError().Return().Action(x =>
        {
            x.Status.IsOk().Should().BeTrue(x.ToString());
            x.Items.Length.Should().Be(1);
            x.Items.OfType<GraphNode>().First().Action(x =>
            {
                x.Key.Should().Be("node1");
                x.Tags.ToTagsString().Should().Be("t1,t2=v1");
            });
        });

        (await testClient.Execute("select (key=node2);", NullScopeContext.Instance)).ThrowOnError().Return().Action(x =>
        {
            x.Status.IsOk().Should().BeTrue(x.ToString());
            x.Items.Length.Should().Be(1);
            x.Items.OfType<GraphNode>().First().Action(x =>
            {
                x.Key.Should().Be("node2");
                x.Tags.ToTagsString().Should().Be("t10,t20=v10");
            });
        });

        (await testClient.ExecuteBatch("delete (key=node1);", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            map.Nodes.Count.Should().Be(1);
            map.Edges.Count.Should().Be(0);
        });

        (await testClient.Execute("select (key=node1);", NullScopeContext.Instance)).ThrowOnError().Return().Action(x =>
        {
            x.Status.IsOk().Should().BeTrue(x.ToString());
            x.Items.Length.Should().Be(0);
        });

        (await testClient.Execute("select (key=node2);", NullScopeContext.Instance)).ThrowOnError().Return().Action(x =>
        {
            x.Status.IsOk().Should().BeTrue(x.ToString());
            x.Items.Length.Should().Be(1);
            x.Items.OfType<GraphNode>().First().Action(x =>
            {
                x.Key.Should().Be("node2");
                x.Tags.ToTagsString().Should().Be("t10,t20=v10");
            });
        });

        (await testClient.ExecuteBatch("delete (key=node2);", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Return().Items.Length.Should().Be(1);
        });

        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task FailOnDuplicateTagKey()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

        string q = """
            add node key=node1, t1;
            add node key=node1,t2,client;
            """;


        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).Action(x =>
        {
            x.IsError().Should().BeTrue(x.ToString());
            x.Value.Items.Length.Should().Be(2);
            x.Value.Items[0].Action(y =>
            {
                y.CommandType.Should().Be(CommandType.AddNode);
                y.Status.IsOk().Should().BeTrue();
                y.Items.Length.Should().Be(0);
            });
            x.Value.Items[1].Action(y =>
            {
                y.CommandType.Should().Be(CommandType.AddNode);
                y.Status.IsConflict().Should().BeTrue();
                y.Items.Length.Should().Be(0);
            });
        });

        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task TwoNodesWithRelationship()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

        string q = """
            add node key=node1, t1;
            add node key=node2,t2,client;
            add edge fromKey=node1,toKey=node2,edgeType=et,e2, worksFor;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Return().Items.Length.Should().Be(3);
        });

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);

        var query = (await testClient.Execute("select (key=node1) a0 -> [*] a1 -> (*) a2;", NullScopeContext.Instance)).ThrowOnError().Return();
        query.Status.IsOk().Should().Be(true);
        query.Items.Length.Should().Be(1);
        query.Items.OfType<GraphNode>().Action(x =>
        {
            x.Count().Should().Be(1);
            x.First().Key.Should().Be("node2");
        });
    }

    [Fact]
    public async Task TwoNodesWithRelationshipLargerSet()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

        string q = """
            add node key=node1,t1;
            add node key=node2,t2,client;
            add node key=node3,t3,client;
            add node key=node4,t4,client;
            add node key=node5,t5,client;
            add edge fromKey=node1,toKey=node2,edgeType=et,e2,worksFor;
            add edge fromKey=node3,toKey=node4,edgeType=et,e2,worksFor;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Items.Length.Should().Be(7);
        });

        map.Nodes.Count.Should().Be(5);
        map.Edges.Count.Should().Be(2);

        (await testClient.Execute("select (key=node3) a0 -> [*] a1 -> (*) a2;", NullScopeContext.Instance)).ThrowOnError().Return().Action(x =>
        {
            x.Status.IsOk().Should().Be(true);
            x.Items.OfType<GraphNode>().Action(x =>
            {
                x.Count().Should().Be(1);
                x.First().Key.Should().Be("node4");
            });
        });
    }
}
