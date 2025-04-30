using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Transactions;

public class GraphTrxNodeTests
{

    [Fact]
    public async Task AddNodeFailure()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            add node key=node1;
            add node key=node2;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).IsOk().BeTrue();

        testClient.Map.Nodes.Count.Be(2);
        testClient.Map.Edges.Count.Be(1);

        string q2 = """
            add node key=node3;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Default)).Action(x =>
        {
            x.IsError().BeTrue();
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });
    }

    [Fact]
    public async Task UpdateNodeFailure()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            add node key=node1;
            add node key=node2;
            set node key=node1 set t1;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).IsOk().BeTrue();

        testClient.Map.Nodes.Count.Be(2);
        testClient.Map.Edges.Count.Be(1);

        string q2 = """
            set node key=node2 set t2;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Default)).Action(x =>
        {
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });
    }

    [Fact]
    public async Task DeleteNodeFailure()
    {
        using GraphHostService testClient = await GraphTestStartup.CreateGraphService();

        string q = """
            add node key=node1;
            add node key=node2;
            add node key=node3;
            set node key=node1 set t1;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q, NullScopeContext.Default)).IsOk().BeTrue();

        testClient.Map.Nodes.Count.Be(3);
        testClient.Map.Edges.Count.Be(1);

        string q2 = """
            delete node key=node3;
            add edge from=node1, to=node2, type=default;
            """;

        (await testClient.ExecuteBatch(q2, NullScopeContext.Default)).Action(x =>
        {
            x.Value.Items.Count.Be(2);
            x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
            x.Value.Items[1].Action(y => TestReturn(y, StatusCode.Conflict));
        });
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Be(statusCode, graphResult.Option.ToString());
    }
}
