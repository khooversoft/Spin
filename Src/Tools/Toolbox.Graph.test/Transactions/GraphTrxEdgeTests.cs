using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Transactions;

public class GraphTrxEdgeTests
{

    [Fact]
    public async Task AddEdgeFailure()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().BeTrue();

        testClient.Map.Nodes.Count.Be(3);
        testClient.Map.Edges.Count.Be(1);

        string q2 = """
            add edge from=node2, to=node3, type=default;
            add node key=node3;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        testClient.Map.Nodes.Count.Be(3);
        testClient.Map.Edges.Count.Be(1);
    }

    [Fact]
    public async Task UpdateEdgeFailure()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            add edge from=node1, to=node2, type=default;
            add edge from=node2, to=node3, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().BeTrue();

        testClient.Map.Nodes.Count.Be(3);
        testClient.Map.Edges.Count.Be(2);

        string q2 = """
            set edge from=node2, to=node3, type=default set t1;
            add node key=node2;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });

        testClient.Map.Nodes.Count.Be(3);
        testClient.Map.Edges.Count.Be(2);
    }

    [Fact]
    public async Task DeleteEdgeFailure()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            set node key=node1 set t1;
            add edge from=node1, to=node2, type=default;
            add edge from=node2, to=node3, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Instance)).IsOk().BeTrue();

        testClient.Map.Nodes.Count.Be(3);
        testClient.Map.Edges.Count.Be(2);

        string q2 = """
            delete edge from=node2, to=node3, type=default;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Instance)).Action(x =>
        {
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });


        testClient.Map.Nodes.Count.Be(3);
        testClient.Map.Edges.Count.Be(2);
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Be(statusCode, graphResult.Option.ToString());
    }
}
