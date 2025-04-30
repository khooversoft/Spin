using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphLifecycleTest
{
    [Fact]
    public async Task SingleNode()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        Option<QueryBatchResult> addResult = await testClient.ExecuteBatch("set node key=node1 set t1,t2=v1;", NullScopeContext.Default);
        addResult.IsOk().BeTrue();

        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Edges.Count.Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Be("node1");
                x.Tags.ToTagsString().Be("t1,t2=v1");
            });
        });

        Option<QueryBatchResult> removeResult = await testClient.ExecuteBatch("delete node key=node1;", NullScopeContext.Default);
        removeResult.IsOk().BeTrue();
        testClient.Map.Nodes.Count.Be(0);
        testClient.Map.Edges.Count.Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });
    }

    [Fact]
    public async Task TwoNodes()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        Option<QueryBatchResult> addResult1 = await testClient.ExecuteBatch("set node key=node1 set t1,t2=v1;", NullScopeContext.Default);
        addResult1.IsOk().BeTrue();

        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Edges.Count.Be(0);

        Option<QueryBatchResult> addResult2 = await testClient.ExecuteBatch("set node key=node2 set t10,t20=v10;", NullScopeContext.Default);
        addResult2.IsOk().BeTrue();
        testClient.Map.Nodes.Count.Be(2);
        testClient.Map.Edges.Count.Be(0);

        (await testClient.Execute("select (key=node1);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Be("node1");
                x.Tags.ToTagsString().Be("t1,t2=v1");
            });
        });

        (await testClient.Execute("select (key=node2);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Be("node2");
                x.Tags.ToTagsString().Be("t10,t20=v10");
            });
        });

        (await testClient.ExecuteBatch("delete node key=node1;", NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().BeTrue();
            testClient.Map.Nodes.Count.Be(1);
            testClient.Map.Edges.Count.Be(0);
        });

        (await testClient.Execute("select (key=node1);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
        });

        (await testClient.Execute("select (key=node2);", NullScopeContext.Default)).ThrowOnError().Return().Action(x =>
        {
            x.Option.IsOk().BeTrue();
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);

            x.Nodes[0].Action(x =>
            {
                x.Key.Be("node2");
                x.Tags.ToTagsString().Be("t10,t20=v10");
            });
        });

        (await testClient.ExecuteBatch("delete node key=node2;", NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Items.Count.Be(1);
        });

        testClient.Map.Nodes.Count.Be(0);
        testClient.Map.Edges.Count.Be(0);
    }

    [Fact]
    public async Task UpdateTags()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            set node key=node1 set t1;
            set node key=node1 set t2,client;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y =>
            {
                y.Option.IsOk().BeTrue();
            });
            x.Value.Items[1].Action(y =>
            {
                y.Option.IsOk().BeTrue();
            });
        });

        testClient.Map.Nodes.Count.Be(1);
        testClient.Map.Nodes["node1"].Action(x =>
        {
            x.Key.Be("node1");
            x.Tags.ToTagsString().Be("client,t1,t2");
        });
        testClient.Map.Edges.Count.Be(0);
    }

    [Fact]
    public async Task TwoNodesWithRelationship()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            set node key=node1 set t1;
            set node key=node2 set t2,client;
            set edge from=node1 ,to=node2, type=et set e2, worksFor;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Items.Count.Be(3);
        });

        testClient.Map.Nodes.Count.Be(2);
        testClient.Map.Edges.Count.Be(1);

        QueryBatchResult batch = (await testClient.ExecuteBatch("select (key=node1) a0 -> [*] a1 -> (*) a2;", NullScopeContext.Default)).ThrowOnError().Return();
        batch.Option.IsOk().Be(true);
        batch.Items.Count.Be(3);

        batch.Items[0].Action(x =>
        {
            x.Alias.Be("a0");
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes[0].Key.Be("node1");
            x.Nodes[0].Tags.ToTagsString().Be("t1");
        });

        batch.Items[1].Action(x =>
        {
            x.Alias.Be("a1");
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(1);
            x.DataLinks.Count.Be(0);
            x.Edges[0].FromKey.Be("node1");
            x.Edges[0].ToKey.Be("node2");
            x.Edges[0].EdgeType.Be("et");
            x.Edges[0].Tags.ToTagsString().Be("e2,worksFor");
        });

        batch.Items[2].Action(x =>
        {
            x.Alias.Be("a2");
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes[0].Key.Be("node2");
            x.Nodes[0].Tags.ToTagsString().Be("client,t2");
        });
    }

    [Fact]
    public async Task TwoNodesWithFullRelationship()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            set node key=node1 set t1;
            set node key=node2 set t2,client;
            set edge from=node1 ,to=node2, type=et set e2, worksFor;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
        {
            x.IsOk().BeTrue();
            x.Return().Items.Count.Be(3);
        });

        testClient.Map.Nodes.Count.Be(2);
        testClient.Map.Edges.Count.Be(1);

        QueryBatchResult batch = (await testClient.ExecuteBatch("select (key=node2) a0 <-> [*] a1 <-> (*) a2;", NullScopeContext.Default)).ThrowOnError().Return();
        batch.Option.IsOk().Be(true);
        batch.Items.Count.Be(3);

        batch.Items[0].Action(x =>
        {
            x.Alias.Be("a0");
            x.Nodes.Count.Be(1);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes[0].Key.Be("node2");
            x.Nodes[0].Tags.ToTagsString().Be("client,t2");
        });

        batch.Items[1].Action(x =>
        {
            x.Alias.Be("a1");
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(1);
            x.DataLinks.Count.Be(0);
            x.Edges[0].FromKey.Be("node1");
            x.Edges[0].ToKey.Be("node2");
            x.Edges[0].EdgeType.Be("et");
            x.Edges[0].Tags.ToTagsString().Be("e2,worksFor");
        });

        batch.Items[2].Action(x =>
        {
            x.Alias.Be("a2");
            x.Nodes.Count.Be(2);
            x.Edges.Count.Be(0);
            x.DataLinks.Count.Be(0);
            x.Nodes.OrderBy(x => x.Key).ToArray().Action(y =>
            {
                y[0].Key.Be("node1");
                y[0].Tags.ToTagsString().Be("t1");
                y[1].Key.Be("node2");
                y[1].Tags.ToTagsString().Be("client,t2");
            });
        });
    }

    [Fact]
    public async Task TwoNodesWithRelationshipLargerSet()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

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
            x.IsOk().BeTrue();
            x.Return().Items.Count.Be(7);
        });

        testClient.Map.Nodes.Count.Be(5);
        testClient.Map.Edges.Count.Be(2);

        (await testClient.ExecuteBatch("select (key=node3) a0 -> [*] a1 -> (*) a2;", NullScopeContext.Default)).ThrowOnError().Return().Action(batch =>
        {
            batch.Option.IsOk().Be(true);
            batch.Items.Count.Be(3);

            batch.Items[0].Action(x =>
            {
                x.Alias.Be("a0");
                x.Nodes.Count.Be(1);
                x.Edges.Count.Be(0);
                x.DataLinks.Count.Be(0);
                x.Nodes[0].Key.Be("node3");
                x.Nodes[0].Tags.ToTagsString().Be("client,t3");
            });

            batch.Items[1].Action(x =>
            {
                x.Alias.Be("a1");
                x.Nodes.Count.Be(0);
                x.Edges.Count.Be(1);
                x.DataLinks.Count.Be(0);
                x.Edges[0].FromKey.Be("node3");
                x.Edges[0].ToKey.Be("node4");
                x.Edges[0].EdgeType.Be("et");
                x.Edges[0].Tags.ToTagsString().Be("e3,worksFor");
            });

            batch.Items[2].Action(x =>
            {
                x.Alias.Be("a2");
                x.Nodes.Count.Be(1);
                x.Edges.Count.Be(0);
                x.DataLinks.Count.Be(0);
                x.Nodes.OrderBy(x => x.Key).ToArray().Action(y =>
                {
                    y[0].Key.Be("node4");
                    y[0].Tags.ToTagsString().Be("client,t4");

                });
            });
        });
    }
}
