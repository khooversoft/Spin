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
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphHost>().Map;

        Option<QueryBatchResult> addResult = await testClient.ExecuteBatch("set node key=node1 set t1,t2=v1;", NullScopeContext.Default);
        addResult.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue();
            x.Nodes.Count.Should().Be(1);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Should().Be("node1");
                x.Tags.ToTagsString().Should().Be("t1,t2=v1");
            });
        });

        Option<QueryBatchResult> removeResult = await testClient.ExecuteBatch("delete node key=node1;", NullScopeContext.Default);
        removeResult.IsOk().Should().BeTrue(removeResult.ToString());
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue();
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task TwoNodes()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphHost>().Map;

        Option<QueryBatchResult> addResult1 = await testClient.ExecuteBatch("set node key=node1 set t1,t2=v1;", NullScopeContext.Default);
        addResult1.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(1);
        map.Edges.Count.Should().Be(0);

        Option<QueryBatchResult> addResult2 = await testClient.ExecuteBatch("set node key=node2 set t10,t20=v10;", NullScopeContext.Default);
        addResult2.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue(x.ToString());
            x.Nodes.Count.Should().Be(1);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Should().Be("node1");
                x.Tags.ToTagsString().Should().Be("t1,t2=v1");
            });
        });

        (await testClient.Execute("select (key=node2);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue(x.ToString());
            x.Nodes.Count.Should().Be(1);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Should().Be("node2");
                x.Tags.ToTagsString().Should().Be("t10,t20=v10");
            });
        });

        (await testClient.ExecuteBatch("delete node key=node1;", NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            map.Nodes.Count.Should().Be(1);
            map.Edges.Count.Should().Be(0);
        });

        (await testClient.Execute("select (key=node1);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue(x.ToString());
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
        });

        (await testClient.Execute("select (key=node2);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().Should().BeTrue(x.ToString());
            x.Nodes.Count.Should().Be(1);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Should().Be("node2");
                x.Tags.ToTagsString().Should().Be("t10,t20=v10");
            });
        });

        (await testClient.ExecuteBatch("delete node key=node2;", NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Return().Items.Count.Should().Be(1);
        });

        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task UpdateTags()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphHost>().Map;

        string q = """
            set node key=node1 set t1;
            set node key=node1 set t2,client;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Value.Items.Count.Should().Be(2);
            x.Value.Items[0].Action(y =>
            {
                y.Option.IsOk().Should().BeTrue();
            });
            x.Value.Items[1].Action(y =>
            {
                y.Option.IsOk().Should().BeTrue();
            });
        });

        map.Nodes.Count.Should().Be(1);
        map.Nodes["node1"].Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("client,t1,t2");
        });
        map.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task TwoNodesWithRelationship()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphHost>().Map;

        string q = """
            set node key=node1 set t1;
            set node key=node2 set t2,client;
            set edge from=node1 ,to=node2, type=et set e2, worksFor;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Return().Items.Count.Should().Be(3);
        });

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);

        QueryBatchResult batch = (await testClient.ExecuteBatch("select (key=node1) a0 -> [*] a1 -> (*) a2;", NullScopeContext.Default)).ThrowOnError().Return();
        batch.Option.IsOk().Should().Be(true);
        batch.Items.Count.Should().Be(3);

        batch.Items[0].Action(x =>
        {
            x.Alias.Should().Be("a0");
            x.Nodes.Count.Should().Be(1);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
            x.Nodes[0].Key.Should().Be("node1");
            x.Nodes[0].Tags.ToTagsString().Should().Be("t1");
        });

        batch.Items[1].Action(x =>
        {
            x.Alias.Should().Be("a1");
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(1);
            x.DataLinks.Count.Should().Be(0);
            x.Edges[0].FromKey.Should().Be("node1");
            x.Edges[0].ToKey.Should().Be("node2");
            x.Edges[0].EdgeType.Should().Be("et");
            x.Edges[0].Tags.ToTagsString().Should().Be("e2,worksFor");
        });

        batch.Items[2].Action(x =>
        {
            x.Alias.Should().Be("a2");
            x.Nodes.Count.Should().Be(1);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
            x.Nodes[0].Key.Should().Be("node2");
            x.Nodes[0].Tags.ToTagsString().Should().Be("client,t2");
        });
    }

    [Fact]
    public async Task TwoNodesWithFullRelationship()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphHost>().Map;

        string q = """
            set node key=node1 set t1;
            set node key=node2 set t2,client;
            set edge from=node1 ,to=node2, type=et set e2, worksFor;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().Should().BeTrue(x.ToString());
            x.Return().Items.Count.Should().Be(3);
        });

        map.Nodes.Count.Should().Be(2);
        map.Edges.Count.Should().Be(1);

        QueryBatchResult batch = (await testClient.ExecuteBatch("select (key=node2) a0 <-> [*] a1 <-> (*) a2;", NullScopeContext.Default)).ThrowOnError().Return();
        batch.Option.IsOk().Should().Be(true);
        batch.Items.Count.Should().Be(3);

        batch.Items[0].Action(x =>
        {
            x.Alias.Should().Be("a0");
            x.Nodes.Count.Should().Be(1);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
            x.Nodes[0].Key.Should().Be("node2");
            x.Nodes[0].Tags.ToTagsString().Should().Be("client,t2");
        });

        batch.Items[1].Action(x =>
        {
            x.Alias.Should().Be("a1");
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(1);
            x.DataLinks.Count.Should().Be(0);
            x.Edges[0].FromKey.Should().Be("node1");
            x.Edges[0].ToKey.Should().Be("node2");
            x.Edges[0].EdgeType.Should().Be("et");
            x.Edges[0].Tags.ToTagsString().Should().Be("e2,worksFor");
        });

        batch.Items[2].Action(x =>
        {
            x.Alias.Should().Be("a2");
            x.Nodes.Count.Should().Be(2);
            x.Edges.Count.Should().Be(0);
            x.DataLinks.Count.Should().Be(0);
            x.Nodes.OrderBy(x => x.Key).ToArray().Action(y =>
            {
                y[0].Key.Should().Be("node1");
                y[0].Tags.ToTagsString().Should().Be("t1");
                y[1].Key.Should().Be("node2");
                y[1].Tags.ToTagsString().Should().Be("client,t2");
            });
        });
    }

    [Fact]
    public async Task TwoNodesWithRelationshipLargerSet()
    {
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphHost>().Map;

        string q = """
            set node key=node1 set t1;
            set node key=node2 set t2,client;
            set node key=node3 set t3,client;
            set node key=node4 set t4,client;
            set node key=node5 set t5,client;
            set edge from=node1,to=node2,type=et set e2,worksFor;
            set edge from=node3,to=node4,type=et set e3,worksFor;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Items.Count.Should().Be(7);
        });

        map.Nodes.Count.Should().Be(5);
        map.Edges.Count.Should().Be(2);

        (await testClient.ExecuteBatch("select (key=node3) a0 -> [*] a1 -> (*) a2;", NullScopeContext.Default)).ThrowOnError().Return().Action(batch =>
        {
            batch.Option.IsOk().Should().Be(true);
            batch.Items.Count.Should().Be(3);

            batch.Items[0].Action(x =>
            {
                x.Alias.Should().Be("a0");
                x.Nodes.Count.Should().Be(1);
                x.Edges.Count.Should().Be(0);
                x.DataLinks.Count.Should().Be(0);
                x.Nodes[0].Key.Should().Be("node3");
                x.Nodes[0].Tags.ToTagsString().Should().Be("client,t3");
            });

            batch.Items[1].Action(x =>
            {
                x.Alias.Should().Be("a1");
                x.Nodes.Count.Should().Be(0);
                x.Edges.Count.Should().Be(1);
                x.DataLinks.Count.Should().Be(0);
                x.Edges[0].FromKey.Should().Be("node3");
                x.Edges[0].ToKey.Should().Be("node4");
                x.Edges[0].EdgeType.Should().Be("et");
                x.Edges[0].Tags.ToTagsString().Should().Be("e3,worksFor");
            });

            batch.Items[2].Action(x =>
            {
                x.Alias.Should().Be("a2");
                x.Nodes.Count.Should().Be(1);
                x.Edges.Count.Should().Be(0);
                x.DataLinks.Count.Should().Be(0);
                x.Nodes.OrderBy(x => x.Key).ToArray().Action(y =>
                {
                    y[0].Key.Should().Be("node4");
                    y[0].Tags.ToTagsString().Should().Be("client,t4");

                });
            });
        });
    }
}
